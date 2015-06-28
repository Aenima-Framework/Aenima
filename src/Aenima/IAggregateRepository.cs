using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima
{
    public interface IAggregateRepository
    {
        Task<T> GetById<T>(string identity, int version) where T : class, IAggregate, new();
        Task Save<T>(T aggregate, IDictionary<string, object> headers = null) where T : class, IAggregate;
    }
}