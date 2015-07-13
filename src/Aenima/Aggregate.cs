using System;
using System.Collections.Generic;
using Aenima.System;

namespace Aenima
{
    public abstract class Aggregate<TState> 
        : IAggregate, IEquatable<IAggregate>
        where TState : class, IState
    {
        private readonly LinkedList<IEvent> changes = new LinkedList<IEvent>();

        protected TState State;

        public string Id => this.State.Id;

        public int Version => this.State.Version;

        protected void Apply(IEvent e)
        {
            this.changes.AddLast(e);
            this.State.Mutate(e);
        }

        IEnumerable<IEvent> IAggregate.GetChanges()
        {
            return this.changes;
        }

        void IAggregate.AcceptChanges()
        {
            this.changes.Clear();
        }

        void IAggregate.Restore(IState state)
        {
            this.State = state as TState;
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
            return $"{GetType().Name}-{Id} v{Version:000}";
        }
    }
}