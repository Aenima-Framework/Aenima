namespace Aenima
{
    public static class StringExtensions
    {
        public static string ToStreamId<TAggregate>(this string id)
        {
            return $"{typeof(TAggregate).Name}-{id}";
        }
    }
}