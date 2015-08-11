using System;
using System.Collections.Generic;
using Aenima.System;

namespace Aenima
{
    public abstract class Aggregate<TState>
        : IAggregate, IEquatable<IAggregate>
        where TState : class, IState
    {
        private readonly LinkedList<IEvent> _changes = new LinkedList<IEvent>();

        protected TState State;
        public string Id => State.Id;
        public int Version => State.Version;

        public TState InternalState => State;

        IEnumerable<IEvent> IAggregate.GetChanges()
        {
            return _changes;
        }

        void IAggregate.AcceptChanges()
        {
            _changes.Clear();
        }

        void IAggregate.Restore(IState state)
        {
            State = state as TState;
        }

        public virtual bool Equals(IAggregate other)
        {
            return null != other && other.GetHashCode() == GetHashCode();
        }

        protected void Apply(IEvent e)
        {
            _changes.AddLast(e);
            State.Mutate(e);
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
            return $"{Id} v{Version}";
        }
    }
}