using System.Collections.Generic;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Extensions;
using Rebus.Pipeline;

namespace Aenima.Rebus
{
    public class RebusEventDispatcher : IEventDispatcher
    {
        private readonly IBus _bus;

        public RebusEventDispatcher(IBus bus)
        {
            _bus = bus;
        }

        public Task Dispatch<TEvent>(TEvent e, IDictionary<string, object> headers = null) where TEvent : class, IEvent
        {
            if(headers != null) {
                foreach(var header in headers) {
                    MessageContext.Current.Message.Headers.Add($"Aenima-{header.Key}", header.Value.ToString());
                }
            }

            return _bus.Publish(e);
        }
    }
}