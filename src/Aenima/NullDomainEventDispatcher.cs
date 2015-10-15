using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public class NullDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task Dispatch<T>(T domainEvent, IDictionary<string, string> headers = null) where T : class
        {
            return Task.CompletedTask;
        }
    }
}