using System;
using System.Collections.Generic;
using Aenima.System;
using Aenima.System.Extensions;

namespace Aenima
{
    public abstract class Aggregate<TState>
        : IAggregate, IEquatable<IAggregate>
        where TState : class, IState, new()
    {
        private readonly LinkedList<object> _changes = new LinkedList<object>();

        public readonly TState State = new TState();

        public string Id => State.Id;
        public int Version => State.Version;

        IEnumerable<object> IAggregate.GetChanges()
        {
            return _changes;
        }

        void IAggregate.AcceptChanges()
        {
            _changes.Clear();
        }

        void IAggregate.Restore(IEnumerable<object> domainEvents)
        {
            domainEvents.WithEach(State.Mutate);
        }

        public virtual bool Equals(IAggregate other)
        {
            return null != other && other.GetHashCode() == GetHashCode();
        }

        protected void Apply(object domainEvent)
        {
            _changes.AddLast(domainEvent);
            State.Mutate(domainEvent);
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

    public abstract class Aggregate
        : IAggregate, IState, IEquatable<IAggregate>
    {
        private readonly LinkedList<object> _changes = new LinkedList<object>();

        public string Id { get; protected set; } = string.Empty;

        public int Version { get; protected set; } = -1;

        public void Mutate(object domainEvent)
        {
            // .NET magic to call one of the 'When' handlers with matching signature 
            ((dynamic)this).When((dynamic)domainEvent);
            Version++;
        }

        public IEnumerable<object> GetChanges()
        {
            return _changes;
        }

        void IAggregate.AcceptChanges()
        {
            _changes.Clear();
        }

        void IAggregate.Restore(IEnumerable<object> domainEvents)
        {
            domainEvents.WithEach(Mutate);
        }

        public virtual bool Equals(IAggregate other)
        {
            return null != other && other.GetHashCode() == GetHashCode();
        }

        protected void Apply(object domainEvent)
        {
            _changes.AddLast(domainEvent);
            Mutate(domainEvent);
        }

        public override int GetHashCode()
        {         
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return HashCodeHelper.CombineHashCodes(GetType(), Id, Version);
            // ReSharper restore NonReadonlyMemberInGetHashCode
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