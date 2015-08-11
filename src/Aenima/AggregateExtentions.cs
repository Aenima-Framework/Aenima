namespace Aenima
{
    public static class AggregateExtentions
    {
        public static string GetStreamId(this IAggregate aggregate)
        {
            return $"{aggregate.GetType().Name}-{aggregate.Id}";
        }
    }
}