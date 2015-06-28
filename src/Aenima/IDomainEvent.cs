using System;

namespace Aenima
{
    /// <summary>
    ///     Interface for events.
    ///     * Can be published.
    ///     * Can be subscribed and unsubscribed to.
    ///     * Cannot be sent using something like IMessageBus.SendCommand() since all events should be published.
    ///     * Cannot implement ICommand.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>Gets the identifier of the domain event.</summary>
        /// <value>The identifier of the domain event.</value>
        Guid Id { get; }

        /// <summary>Gets the Date/Time of when the event was raised.</summary>
        /// <value>The Date/Time of when the event raised.</value>
        DateTime RaisedOn { get; }

        /// <summary>Gets the identifier of the aggregate.</summary>
        /// <value>The identifier of the aggregate.</value>
        string AggregateId { get; }

        /// <summary>Gets the version of the aggregate.</summary>
        /// <value>The version of the aggregate.</value>
        int AggregateVersion { get; }

        /// <summary>Gets the identifier of the long running process where this event was generated.</summary>
        /// <value>The identifier of the long running process where this event was generated.</value>
        Guid? ProcessId { get; }

        void SetMetadata(Guid id, DateTime raisedOn, string aggregateId, int aggregateVersion, Guid? processId = null);
    }
}