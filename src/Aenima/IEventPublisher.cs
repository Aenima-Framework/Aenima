using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IEventPublisher
    {
        Task Publish<TEvent>(TEvent e, IDictionary<string, object> headers = null) where TEvent : class, IEvent;
    }
}