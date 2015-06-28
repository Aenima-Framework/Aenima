using System;
using System.Collections.Generic;
using Aenima.System;
using Aenima.System.Extensions;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Aenima
{
    public abstract class Aggregate
        : IAggregate, IEquatable<IAggregate>
    {
        private readonly LinkedList<IDomainEvent> changes = new LinkedList<IDomainEvent>();

        protected Aggregate()
        {
            Version = -1;
        }

        public string Id { get; private set; }
        public int Version { get; private set; }

        public int ChangedVersion {
            get { return Version + this.changes.Count; }
        }

        void Mutate(IDomainEvent domainEvent)
        {
            // .NET magic to call one of the 'When' handlers with matching signature 
            ((dynamic)this).When((dynamic)domainEvent);
        }

        void IAggregate.Apply(IDomainEvent domainEvent)
        {
            // pass each event to modify current in-memory state
            Mutate(domainEvent);

            // add metadata
            domainEvent.SetMetadata(
                id              : SequentialGuid.New(), 
                raisedOn        : DateTime.UtcNow, 
                aggregateId     : Id, 
                aggregateVersion: ChangedVersion + 1);

            // append event to change list for further persistence
            lock(this.changes) 
            {
                this.changes.AddLast(domainEvent);
            }
        }

        void IAggregate.Hydrate(IEnumerable<IDomainEvent> events)
        {
            foreach(var e in events)
            {
                Mutate(e);
                Version++;
            }
        }

        void IAggregate.AcceptChanges()
        {
            lock(this.changes)
            {
                Version = ChangedVersion;
                this.changes.Clear();
            }
        }

        IEnumerable<IDomainEvent> IAggregate.GetChanges()
        {
            lock(this.changes)
            {
                return this.changes;
            }
        }

        public virtual bool Equals(IAggregate other)
        {
            return null != other && other.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(GetType(), Id, Version);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IAggregate);
        }

        public override string ToString()
        {
            return "{0}-{1:000}".FormatWith(Id, Version);
        }
    }
}