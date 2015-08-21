using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public class NullEventDispatcher : IEventDispatcher
    {
        public Task Dispatch<T>(T e, IDictionary<string, string> headers = null) where T : class, IEvent
        {
            return Task.FromResult(0);
        }
    }
}