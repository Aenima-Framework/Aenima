using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IEventDispatcher
    {
        Task Dispatch<T>(T e, IDictionary<string, string> headers = null) where T : class, IEvent;
    }
}