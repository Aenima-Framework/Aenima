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
        public TAggregate Create<TAggregate>(IEnumerable<IEvent> events)
            where TAggregate : class, IAggregate
        {
            var aggregateType      = typeof(TAggregate);
            var aggregateStateType = Type.GetType($"{aggregateType.Name}State", false);

            if(aggregateStateType == null) {
                throw new InvalidOperationException($"Failed to find state for \"{aggregateType.Name}\" aggregate.");
            }

            var state = (IState)Activator.CreateInstance(aggregateStateType);
            events.WithEach(state.Mutate);

            var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), state);
            aggregate.Restore(state);

            return aggregate;
        }
    }
}