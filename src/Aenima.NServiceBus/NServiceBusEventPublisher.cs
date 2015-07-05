using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using static System.String;

namespace Aenima.NServiceBus
{
    public class NServiceBusEventPublisher : IEventPublisher
    {
        private readonly IBus bus;

        public NServiceBusEventPublisher(IBus bus)
        {
            this.bus = bus;
        }

        public Task Publish<TEvent>(TEvent e, IDictionary<string, object> headers = null) where TEvent : class, IEvent
        {
            if(headers != null) {
                foreach(var header in headers) {
                    this.bus.SetMessageHeader(e, $"Aenima-{header.Key}", header.Value.ToString());
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
            var header = $"Aenima-{key}";
            return bus.CurrentMessageContext.Headers.ContainsKey(header)
                ? bus.CurrentMessageContext.Headers[header]
                : Empty;
        }
    }
}