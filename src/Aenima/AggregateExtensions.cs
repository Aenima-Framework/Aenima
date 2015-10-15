namespace Aenima
{
    public static class AggregateExtensions
    {
        public static string GetStreamId(this IAggregate aggregate)
        {
            return $"{aggregate.GetType().Name}-{aggregate.Id}";
        }
    }
}