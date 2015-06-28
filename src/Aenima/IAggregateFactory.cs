using System.Collections.Generic;

namespace Aenima
{
    public interface IAggregateFactory
    {
        T Create<T>(IEnumerable<IDomainEvent> events) where T : class, IAggregate;
    }
}