using System.Collections.Generic;

namespace Aenima
{
    public interface IAggregateFactory {
        TAggregate Create<TAggregate>(IEnumerable<IEvent> events)
            where TAggregate : class, IAggregate;
    }
}