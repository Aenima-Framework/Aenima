using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IEventDispatcher
    {
        Task Publish<TEvent>(TEvent e, IDictionary<string, object> headers = null) where TEvent : class, IEvent;
    }
}