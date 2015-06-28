using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;

namespace Aenima.NServiceBus
{
    public class NServiceBusDomainEventPublisher : IDomainEventPublisher
    {
        private readonly IBus bus;

        public NServiceBusDomainEventPublisher(IBus bus)
        {
            this.bus = bus;
        }

        public Task Publish<TEvent>(TEvent message, IDictionary<string, object> headers = null) where TEvent : class, IDomainEvent
        {
            //attach all headers
            //this.bus.SetMessageHeader("","");
            this.bus.Publish(message);

            return Task.FromResult(0);
        }
    }
}
