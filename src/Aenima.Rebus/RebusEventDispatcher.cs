using System.Collections.Generic;
using System.Threading.Tasks;
using Rebus;
using static System.String;

namespace Aenima.Rebus
{
    public class RebusEventDispatcher : IEventDispatcher
    {
        private readonly IBus bus;

        public RebusEventDispatcher(IBus bus)
        {
            this.bus = bus;
        }

        public Task Publish<TEvent>(TEvent e, IDictionary<string, object> headers = null) where TEvent : class, IEvent
        {
            if(headers != null) {
                foreach(var header in headers) {
                    this.bus.AttachHeader(e, $"Aenima-{header.Key}", header.Value.ToString());
                }
            }

            this.bus.Publish(e);

            return Task.FromResult(0);
        }
    }

    public static class BusExtensions
    {
        public static string GetAenimaHeader(this IBus bus, string key)
        {
            var header     = $"Aenima-{key}";
            var msgContext = MessageContext.GetCurrent();

            return msgContext.Headers.ContainsKey(header)
                ? msgContext.Headers[header].ToString()
                : Empty;
        }
    }
}