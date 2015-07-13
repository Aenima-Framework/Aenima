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
        private readonly IEventSerializer serializer;
        private readonly IEventPublisher publisher;
        private readonly IAggregateFactory aggregateFactory;

        public Repository(
            IEventStore store,
            IEventSerializer serializer,
            IEventPublisher publisher,
            IAggregateFactory aggregateFactory)
        {
            this.store            = store;
            this.serializer       = serializer;
            this.publisher        = publisher;
            this.aggregateFactory = aggregateFactory;
        }

        public async Task<TAggregate> GetById<TAggregate>(string id, int version) where TAggregate : class, IAggregate, new()
        {
            Guard.NullOrWhiteSpace(() => id);

            var streamId   = GetStreamId<TAggregate>(id);
            var pageStart  = 0;
            var events     = new List<IEvent>();

            try {
                StreamEventsPage currentPage;
                do {
                    var eventCount = pageStart + PageSizeReadBuffer <= version
                        ? PageSizeReadBuffer
                        : version - pageStart + 1;

                    currentPage = await this.store.ReadStream(streamId, pageStart, eventCount);

                    events.AddRange(
                        currentPage.Events.Select(streamEvent =>
                            this.serializer.FromStreamEvent<IEvent>(streamEvent)));

                    pageStart = currentPage.NextVersion;
                }
                while(version >= currentPage.NextVersion && !currentPage.IsEndOfStream);
            }
            catch(StreamNotFoundException ex) {
                throw new AggregateNotFoundException<TAggregate>(id, ex);
            }
            catch(StreamDeletedException ex) {
                throw new AggregateDeletedException<TAggregate>(id, version, ex);
            }

            var aggregate = this.aggregateFactory.Create<TAggregate>(events);

            if(aggregate.Version != version && version < int.MaxValue) {
                throw new AggregateConcurrencyException<TAggregate>(id, version, aggregate.Version);
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

            var events = aggregate
                .GetChanges()
                .Select((e, idx) => {
                    var eventMetadata = new Dictionary<string, object> {
                        { Headers.EventId         , SequentialGuid.New() },
                        { Headers.CommitId        , commitId },
                        { Headers.AggregateVersion, aggregate.Version + idx + 1 },
                        { Headers.EventClrType    , e.GetType().AssemblyQualifiedName },
                        { Headers.AggregateClrType, aggregateType.AssemblyQualifiedName },
                    };
                    return this.serializer.ToNewStreamEvent(e, eventMetadata.Merge(headers));
                });

            try {
                await this.store.AppendStream(streamId, aggregate.Version, events);
            }
            catch(StreamConcurrencyException ex) {
                throw new AggregateConcurrencyException<TAggregate>(aggregate.Id, aggregate.Version, ex.ActualVersion);
            }

            foreach(var e in aggregate.GetChanges()) {
                try {
                    await this.publisher.Publish(e);
                }
                catch(Exception ex) {
                    this.log.Warning(ex, "Failed to publish {@Event}!", e);
                }
            }

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


    public static class Headers
    {
        public const string CommitId         = "CommitId";
        public const string EventId          = "EventId";
        public const string EventClrType     = "EventClrType";
        public const string AggregateVersion = "AggregateVersion";
        public const string AggregateClrType = "AggregateClrType";
    }
}