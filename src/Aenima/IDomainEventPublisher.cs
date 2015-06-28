using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IDomainEventPublisher
    {
        Task Publish<T>(T domainEvent, IDictionary<string, object> headers = null) where T : class, IDomainEvent;
    }
}