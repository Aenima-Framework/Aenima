using System.Collections.Generic;

namespace Aenima
{
    public interface IAggregate
    {
        string Id { get; }

        int Version { get; }

        /// <summary>
        /// Mutates the aggregate and stores the event as uncommitted.
        /// </summary>
        /// <param name="domainEvent">The IDomainEvent to handle.</param>
        void Apply(IDomainEvent domainEvent);

        /// <summary>
        /// Hydrates the aggregate by mutating it with a list of events in order.
        /// </summary>
        /// <param name="events">The events to mutate the aggregate.</param>
        void Hydrate(IEnumerable<IDomainEvent> events);

        /// <summary>
        /// Gets all the uncommitted events.
        /// </summary>
        /// <returns>
        /// The events that affected the aggregate.
        /// </returns>
        IEnumerable<IDomainEvent> GetChanges();

        /// <summary>
        /// Clears the list of uncommitted events and upgrades the version of the aggregate.
        /// </summary>
        void AcceptChanges();
    }
}