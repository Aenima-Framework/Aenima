using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IDomainEventDispatcher
    {
        Task Dispatch<T>(T domainEvent, IDictionary<string, string> headers = null) where T : class;
    }
}