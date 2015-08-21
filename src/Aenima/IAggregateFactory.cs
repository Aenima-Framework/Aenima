using System.Collections.Generic;
using System.Linq;

namespace Aenima
{
    public interface IAggregateFactory
    {
        TAggregate Create<TAggregate>() where TAggregate : class, IAggregate;
        TAggregate Create<TAggregate>(IEnumerable<object> events) where TAggregate : class, IAggregate;
    }

    public static class AggregateFactoryExtensions
    {
        public static TAggregate Create<TAggregate>(this IAggregateFactory factory, params object[] events) where TAggregate : class, IAggregate
        {
            return factory.Create<TAggregate>(events.ToList());
        }
    }
}