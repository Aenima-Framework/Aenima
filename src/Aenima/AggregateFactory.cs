using System;
using System.Collections.Generic;
using Aenima.System.Extensions;

namespace Aenima
{
    /// <summary>
    /// A factory that creates aggregates from events, resolving state type by naming
    /// convention - "{AggregateName}State". It must implements IState.
    /// </summary>
    public class AggregateFactory : IAggregateFactory
    {
        public static IAggregateFactory Instance = new AggregateFactory();

        public TAggregate Create<TAggregate>()
            where TAggregate : class, IAggregate
        {
            var aggregateType      = typeof(TAggregate);
            var aggregateStateType = Type.GetType(aggregateType.BaseType.GenericTypeArguments[0].AssemblyQualifiedName, false);

            if(aggregateStateType == null) {
                throw new InvalidOperationException($"Failed to find state for \"{aggregateType.Name}\" aggregate.");
            }

            var state     = (IState)Activator.CreateInstance(aggregateStateType);
            var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate));

            aggregate.Restore(state);

            return aggregate;
        }

        public TAggregate Create<TAggregate>(IEnumerable<object> events)
            where TAggregate : class, IAggregate
        {
            var aggregateType      = typeof(TAggregate);
            var aggregateStateType = Type.GetType(aggregateType.BaseType.GenericTypeArguments[0].AssemblyQualifiedName, false);

            if(aggregateStateType == null) {
                throw new InvalidOperationException($"Failed to find state for \"{aggregateType.Name}\" aggregate.");
            }

            var state = (IState)Activator.CreateInstance(aggregateStateType);
            events.WithEach(state.Mutate);

            var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate));
            aggregate.Restore(state);

            return aggregate;
        }
    }
}