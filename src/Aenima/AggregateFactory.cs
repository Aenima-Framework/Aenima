namespace Aenima
{
    public static class AggregateFactory
    {
        public static TAggregate Create<TAggregate>(params object[] domainEvents)
            where TAggregate : class, IAggregate, new()
        {
            var aggregateRoot = new TAggregate();

            if (domainEvents != null) {
                aggregateRoot.Restore(domainEvents);
            }

            return aggregateRoot;
        }
    }
}