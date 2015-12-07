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

        private readonly IEventStore _store;     

        public Repository(IEventStore store)
        {
            _store = store;
        }

        public async Task<TAggregate> GetById<TAggregate>(string id, int version)
            where TAggregate : class, IAggregate, new()
        {
            Block.NullOrWhiteSpace(() => id);
            Block.SmallerThan(() => version, 0);

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

            var aggregate = AggregateFactory.Create<TAggregate>(events);

            if(aggregate.Version != version && version < int.MaxValue) {
                throw new StreamConcurrencyException(streamId, version, aggregate.Version);
            }

            await _log.Debug("Aggregate {Id} of type {Type} loaded up to v{Version},", id, typeof(TAggregate).Name, version);

            return aggregate;
        }

        public async Task Save<TAggregate>(TAggregate aggregate, IDictionary<string, string> headers = null)
            where TAggregate : class, IAggregate
        {
            Block.Null(() => aggregate);

            await _log.Debug("Saving aggregate {@Aggregate}", aggregate.ToString());

            if(!aggregate.GetChanges().Any()) {
                await _log.Debug("Aggregate {@Aggregate} has no changes. Skipping...", aggregate.ToString());
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
}