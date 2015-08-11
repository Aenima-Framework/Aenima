using Rebus.Bus;
using Rebus.Pipeline;
using static System.String;

namespace Aenima.Rebus
{
    public static class BusExtensions
    {
        public static string GetAenimaHeader(this IBus bus, string key)
        {
            var header = $"Aenima-{key}";
            return MessageContext.Current.Message.Headers.ContainsKey(header)
                ? MessageContext.Current.Message.Headers[header]
                : Empty;
        }
    }
}