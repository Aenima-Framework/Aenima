using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Aenima.EventStore;
using Aenima.Exceptions;
using Aenima.Logging;
using Aenima.System;
using Aenima.System.Extensions;

namespace Aenima
{
    public class Repository : IRepository
    {
        private readonly ILog _log = Log.ForContext<Repository>();

        private const int PageSizeReadBuffer = 200; //TODO: move to configuration

        private readonly IAggregateFactory _aggregateFactory;
        private readonly IEventStore _store;     

        public Repository(
            IEventStore store,
            IAggregateFactory aggregateFactory)
        {
            _store = store;
            _aggregateFactory = aggregateFactory;
        }

        public async Task<TAggregate> GetById<TAggregate>(string id, int version)
            where TAggregate : class, IAggregate, new()
        {
            Guard.NullOrWhiteSpace(() => id);

            await _log.Debug("Loading Aggregate {@Id} of type {Type}, up to version {Version}", id, typeof(TAggregate).Name, version);

            var streamId  = id.ToStreamId<TAggregate>();
            var pageStart = 0;
            var events    = new List<object>();

            StreamEventsPage currentPage;
            do {
                var eventCount = pageStart + PageSizeReadBuffer <= version
                    ? PageSizeReadBuffer
                    : version - pageStart + 1;

                currentPage = await _store
                    .ReadStream(streamId, pageStart, eventCount)
                    .ConfigureAwait(false);

                events.AddRange(currentPage.Events.Select(streamEvent => streamEvent.Event));

                pageStart = currentPage.NextVersion;
            } while(version >= currentPage.NextVersion && !currentPage.IsEndOfStream);

            var aggregate = _aggregateFactory.Create<TAggregate>(events);

            if(aggregate.Version != version && version < int.MaxValue) {
                throw new StreamConcurrencyException(streamId, version, aggregate.Version);
            }

            await _log.Debug("Aggregate {Id} of type {Type} loaded up to v{Version},", id, typeof(TAggregate).Name, version);

            return aggregate;
        }

        public async Task Save<TAggregate>(TAggregate aggregate, IDictionary<string, string> headers = null)
            where TAggregate : class, IAggregate
        {
            Guard.NullOrDefault(() => aggregate);

            await _log.Debug("Saving aggregate {@Aggregate}", aggregate.ToString());

            if(!aggregate.GetChanges().Any()) {
                return;
            }

            var commitId = Guid.NewGuid().ToString();

            var streamEvents = aggregate
                .GetChanges()
                .Select(
                    (domainEvent, idx) => {
                        var eventMetadata = new Dictionary<string, string> {
                            {EventMetadataKeys.Id         , Guid.NewGuid().ToString()},
                            {EventMetadataKeys.ClrType    , domainEvent.GetType().AssemblyQualifiedName},
                            {EventMetadataKeys.RaisedOn   , DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)},
                            {EventMetadataKeys.AggregateId, aggregate.Id},
                            {EventMetadataKeys.Version    , (aggregate.Version + idx).ToString()},
                            {EventMetadataKeys.Owner      , typeof(TAggregate).Name},
                            {EventMetadataKeys.CommitId   , commitId}
                        };
                        return new StreamEvent(domainEvent, eventMetadata.Merge(headers));
                    })
                .ToList();

            await _store
                .AppendStream(
                    streamId       : aggregate.GetStreamId(), 
                    expectedVersion: aggregate.Version-1, 
                    streamEvents   : streamEvents)
                .ConfigureAwait(false);

            aggregate.AcceptChanges();

            await _log.Debug("Aggregate {@Aggregate} saved", aggregate.ToString());
        }
    }


    public class SnapshotRepository : IRepository
    {
        public Task<TAggregate> GetById<TAggregate>(string identity, int version) where TAggregate : class, IAggregate, new()
        {
            throw new NotImplementedException();
        }

        public Task Save<T>(T aggregate, IDictionary<string, string> headers = null) where T : class, IAggregate
        {
            throw new NotImplementedException();
        }
    }

    public enum ReadDirection
    {
        /// <summary>
        /// From beginning to end.
        /// </summary>
        Forward,
        /// <summary>
        /// From end to beginning.
        /// </summary>
        Backward
    }

    public interface ISnapshotStore
    {
        Task AddSnapshot(
            string aggregateId,
            long expectedVersion,
            object state,
            IDictionary<string, string> metadata);

        Task UpdateSnapshot(
            string aggregateId,
            long expectedVersion,
            object state,
            IDictionary<string, string> metadata);

        Task<Snapshot> GetSnapshot(string aggregateId, long version);

        Task<Snapshot> GetLatestSnapshot(string aggregateId);

        Task<Snapshot> GetAllSnapshots(
            string aggregateId,
            long fromVersion,
            int count,
            ReadDirection direction);

        Task DeleteSnapshot(string aggregateId, long version);

        Task DeleteAllSnapshots(string aggregateId, long fromVersion, long toVersion);

        Task Initialize();
    }

    public static class SnapshotStoreExtensions
    {
    //    public static Task AddSnapshot(this ISnapshotStore store, Snapshot snapshot)
    //    {
    //        return store.AddSnapshot(
    //            snapshot.AggregateState.Id, 
    //            snapshot.AggregateState.Version, 
    //            snapshot.Metadata);
    //    }
    }

    public class Snapshot
    {
        public readonly object State;
        public readonly IDictionary<string, string> Metadata;

        public Snapshot(object state, IDictionary<string, string> metadata)
        {
            State    = state;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}