using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Extensions;

namespace Aenima.Rebus
{
    public class RebusEventDispatcher : IEventDispatcher
    {
        private readonly IBus _bus;

        public RebusEventDispatcher(IBus bus)
        {
            _bus = bus;
        }

        public Task Dispatch<TEvent>(TEvent e, IDictionary<string, string> headers = null) where TEvent : class, IEvent
        {
            return _bus.Publish(e, headers?.ToDictionary(header => $"Aenima-{header.Key}", header => header.Value));
        }
    }
}