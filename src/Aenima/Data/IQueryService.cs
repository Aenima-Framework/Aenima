using System.Threading;
using System.Threading.Tasks;

namespace Aenima.Data
{
    public interface IQueryService
    {
        Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
    }
}