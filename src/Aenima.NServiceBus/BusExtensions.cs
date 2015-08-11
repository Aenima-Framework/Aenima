using NServiceBus;
using static System.String;

namespace Aenima.NServiceBus
{
    public static class BusExtensions
    {
        public static string GetAenimaHeader(this IBus bus, string key)
        {
            var header = $"Aenima-{key}";
            return bus.CurrentMessageContext.Headers.ContainsKey(header)
                ? bus.CurrentMessageContext.Headers[header]
                : Empty;
        }
    }
}