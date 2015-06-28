using System.Collections.Generic;
using System.Threading.Tasks;
using Rebus;

namespace Aenima.Rebus
{
    public class RebusDomainEventPublisher : IDomainEventPublisher
    {
        private readonly IBus bus;

        public RebusDomainEventPublisher(IBus bus)
        {
            this.bus = bus;
        }

        public Task Publish<TEvent>(TEvent message, IDictionary<string, object> headers = null) where TEvent : class, IDomainEvent
        {
            //attach all headers
            //this.bus.AttachHeader(message, "",message.ProcessId);
            this.bus.Publish(message);

            return Task.FromResult(0);
        }
    }
}