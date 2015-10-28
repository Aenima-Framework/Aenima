using System;
using System.Threading;
using System.Threading.Tasks;
using Aenima.DependencyResolution;

namespace Aenima.Data
{
    public class InProcQueryService : IQueryService
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly Type _handlerInterfaceType = typeof(IQueryHandler<,>);

        public InProcQueryService(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = new CancellationToken())
        {
            var handlerType = _handlerInterfaceType.MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _dependencyResolver.Resolve(handlerType);

            return await handler
                .Handle((dynamic)query, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}