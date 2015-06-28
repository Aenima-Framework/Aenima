using System;

namespace Aenima
{
    [Serializable]
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>Gets the identifier of the domain event.</summary>
        /// <value>The identifier of the domain event.</value>
        public Guid Id { get; internal set; }

        /// <summary>Gets the Date/Time of when the event was raised.</summary>
        /// <value>The Date/Time of when the event raised.</value>
        public DateTime RaisedOn { get; internal set; }

        /// <summary>Gets the identifier of the aggregate.</summary>
        /// <value>The identifier of the aggregate.</value>
        public string AggregateId { get; internal set; }

        /// <summary>Gets the version of the aggregate.</summary>
        /// <value>The version of the aggregate.</value>
        public int AggregateVersion { get; internal set; }

        /// <summary>Gets the identifier of the long running process where this event was generated.</summary>
        /// <value>The identifier of the long running process where this event was generated.</value>
        public Guid? ProcessId { get; internal set; }

        void IDomainEvent.SetMetadata(Guid id, DateTime raisedOn, string aggregateId, int aggregateVersion, Guid? processId = null)
        {
            Id               = id;
            RaisedOn         = raisedOn;
            AggregateId      = aggregateId;
            AggregateVersion = aggregateVersion;
            ProcessId        = processId;
        }
    }
}