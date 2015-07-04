using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aenima.EventStore;
using Aenima.Exceptions;
using Aenima.System;
using Aenima.System.Extensions;

namespace Aenima
{
    public class AggregateRepository : IAggregateRepository
    {
        private const int PageSizeReadBuffer = 200;

        private readonly IEventStore eventStore;
        private readonly IDomainEventPublisher domainEventPublisher;
        private readonly IDomainEventSerializer serializer;

        public AggregateRepository(
            IEventStore eventStore,
            IDomainEventSerializer serializer,
            IDomainEventPublisher domainEventPublisher = null)
        {
            this.eventStore           = eventStore;
            this.domainEventPublisher = domainEventPublisher;
            this.serializer           = serializer;
        }

        public async Task<TAggregate> GetById<TAggregate>(string id, int version) where TAggregate : class, IAggregate, new()
        {
            Guard.NullOrWhiteSpace(() => id);

            var streamName = "{0}-{1}".FormatWith(typeof(TAggregate), id);
            var aggregate = new TAggregate();
            var pageStart = 0;

            try {
                StreamEventsPage currentPage;
                do {
                    var eventCount = pageStart + PageSizeReadBuffer <= version
                        ? PageSizeReadBuffer
                        : version - pageStart + 1;

                    currentPage = await this.eventStore.ReadStream(streamName, pageStart, eventCount);

                    aggregate.Hydrate(
                        currentPage.Events.Select(streamEvent => this.serializer.FromStreamEvent(streamEvent)));

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

            // TODO: should probably validate before loading events.
            if(aggregate.Version != version && version < int.MaxValue) {
                throw new AggregateConcurrencyException<TAggregate>(id, version, aggregate.Version);
            }

            return aggregate;
        }

        public async Task Save<TAggregate>(TAggregate aggregate, IDictionary<string, object> headers = null)
            where TAggregate : class, IAggregate
        {
            Guard.NullOrDefault(() => aggregate);

            if(!aggregate.GetChanges()
                .Any()) {
                return;
            }

            // set headers
            var aggregateType = typeof(TAggregate);

            var defaultHeaders = new Dictionary<string, object>
            {
                { "StreamId", "{0}-{1}".FormatWith(aggregateType.Name, aggregate.Id) },
                { "CommitId", SequentialGuid.New() },
                { "AggregateTypeName", aggregateType.Name },
                { "AggregateClrType", aggregateType }
            };

            var events = aggregate
                .GetChanges()
                .Select(
                    domainEvent => this.serializer.ToNewStreamEvent(
                        domainEvent,
                        defaultHeaders.Merge(headers)));

            try {
                await this.eventStore.AppendStream(aggregate.Id, aggregate.Version, events);
            }
            catch(StreamConcurrencyException ex) {
                throw new AggregateConcurrencyException<TAggregate>(aggregate.Id, aggregate.Version, ex.ActualVersion);
            }

            aggregate.GetChanges()
                .WithEach(
                    e => { this.domainEventPublisher.Publish(e); }
                );

            aggregate.AcceptChanges();
        }
    }
}