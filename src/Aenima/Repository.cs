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
        private readonly ILog log = Log.ForContext<Repository>();

        private const int PageSizeReadBuffer = 200;

        private readonly IEventStore store;
        private readonly IAggregateFactory aggregateFactory;

        public Repository(
            IEventStore store,
            IAggregateFactory aggregateFactory)
        {
            this.store            = store;
            this.aggregateFactory = aggregateFactory;
        }

        public async Task<TAggregate> GetById<TAggregate>(string id, int version) 
            where TAggregate : class, IAggregate, new()
        {
            Guard.NullOrWhiteSpace(() => id);

            var streamId   = GetStreamId<TAggregate>(id);
            var pageStart  = 0;
            var events     = new List<IEvent>();

                StreamEventsPage currentPage;
                do {
                    var eventCount = pageStart + PageSizeReadBuffer <= version
                        ? PageSizeReadBuffer
                        : version - pageStart + 1;

                    currentPage = await this.store.ReadStream(streamId, pageStart, eventCount);

                    events.AddRange(currentPage.Events.Select(streamEvent => streamEvent.Event));

                    pageStart = currentPage.NextVersion;
                }
                while(version >= currentPage.NextVersion && !currentPage.IsEndOfStream);

            var aggregate = this.aggregateFactory.Create<TAggregate>(events);

            if(aggregate.Version != version && version < int.MaxValue) {
                throw new StreamConcurrencyException(streamId, version, aggregate.Version);
            }

            return aggregate;
        }

        public async Task  Save<TAggregate>(TAggregate aggregate, IDictionary<string, object> headers = null)
            where TAggregate : class, IAggregate
        {
            Guard.NullOrDefault(() => aggregate);

            if(!aggregate.GetChanges().Any())
                return;

            var aggregateType = typeof(TAggregate);
            var streamId      = GetStreamId(aggregateType, aggregate.Id);
            var commitId      = SequentialGuid.New();

            var streamEvents = aggregate
               .GetChanges()
               .Select((e, idx) => {
                    var eventMetadata = new Dictionary<string, object> {
                        { EventMetadataKeys.Id          , Guid.NewGuid() },
                        { EventMetadataKeys.ClrType     , e.GetType().AssemblyQualifiedName },
                        { EventMetadataKeys.RaisedOn    , DateTime.UtcNow },
                        { EventMetadataKeys.AggregateId , aggregate.Id },
                        { EventMetadataKeys.Version     , aggregate.Version + idx + 1 },
                        { EventMetadataKeys.Owner       , aggregateType.Name },
                        { EventMetadataKeys.CommitId    , commitId },
                    };
                    return new StreamEvent(e, eventMetadata.Merge(headers));
                })
                .ToList();

            await this.store.AppendStream(streamId, aggregate.Version, streamEvents);

            aggregate.AcceptChanges();
        }

        private static string GetStreamId<T>(string id)
        {
            return GetStreamId(typeof(T), id);
        }

        private static string GetStreamId(Type type, string id)
        {
            return $"{type.Name}-{id}";
        }
    }
}