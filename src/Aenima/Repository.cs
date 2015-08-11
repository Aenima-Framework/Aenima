using System;
using System.Collections.Generic;
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
            var events    = new List<IEvent>();

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

        public async Task Save<TAggregate>(TAggregate aggregate, IDictionary<string, object> headers = null)
            where TAggregate : class, IAggregate
        {
            Guard.NullOrDefault(() => aggregate);

            await _log.Debug("Saving aggregate {@Aggregate}", aggregate.ToString());

            if(!aggregate.GetChanges().Any()) {
                return;
            }

            var commitId = SequentialGuid.New();

            var streamEvents = aggregate
                .GetChanges()
                .Select(
                    (e, idx) => {
                        var eventMetadata = new Dictionary<string, object> {
                            {EventMetadataKeys.Id         , SequentialGuid.New()},
                            {EventMetadataKeys.ClrType    , e.GetType().AssemblyQualifiedName},
                            {EventMetadataKeys.RaisedOn   , DateTime.UtcNow},
                            {EventMetadataKeys.AggregateId, aggregate.Id},
                            {EventMetadataKeys.Version    , aggregate.Version + idx},
                            {EventMetadataKeys.Owner      , typeof(TAggregate).Name},
                            {EventMetadataKeys.CommitId   , commitId}
                        };
                        return new StreamEvent(e, eventMetadata.Merge(headers));
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
}