using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Unicast;

namespace Aenima.NServiceBus
{
    public class NServiceBusEventDispatcher : IEventDispatcher
    {
        private readonly IBus _bus;

        public NServiceBusEventDispatcher(IBus bus)
        {
            _bus = bus;
        }

        public Task Dispatch<T>(T e, IDictionary<string, string> headers = null) where T : class, IEvent
        {
            if(headers != null) {
                foreach(var header in headers) {
                    _bus.SetMessageHeader(e, $"Aenima-{header.Key}", header.Value);
                }
            }
            _bus.Publish(e);

            return Task.CompletedTask;
        }
    }
}