using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Extensions;

namespace Aenima.Rebus
{
    public class RebusDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IBus _bus;

        public RebusDomainEventDispatcher(IBus bus)
        {
            _bus = bus;
        }

        public Task Dispatch<T>(T domainEvent, IDictionary<string, string> headers = null) where T : class
        {
            return _bus.Publish(domainEvent, headers?.ToDictionary(header => $"Aenima-{header.Key}", header => header.Value));
        }
    }
}