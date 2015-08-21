using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;

namespace Aenima.NServiceBus
{
    public class NServiceBusDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IBus _bus;

        public NServiceBusDomainEventDispatcher(IBus bus)
        {
            _bus = bus;
        }

        public Task Dispatch<T>(T domainEvent, IDictionary<string, string> headers = null) where T : class
        {
            if(headers != null) {
                foreach(var header in headers) {
                    _bus.SetMessageHeader(domainEvent, $"Aenima-{header.Key}", header.Value);
                }
            }
            _bus.Publish(domainEvent);

            return Task.CompletedTask;
        }
    }
}