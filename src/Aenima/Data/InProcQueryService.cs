using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Aenima.DependencyResolution;
using Aenima.System.Extensions;

namespace Aenima.Data
{
    public class QueryService : IQueryService
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly Type _handlerInterfaceType = typeof(IQueryHandler<,>);

        public QueryService(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public async Task<TResult> Run<TQuery,TResult>(TQuery query, CancellationToken cancellationToken = new CancellationToken()) 
            where TQuery : IQuery<TResult>
        {
            return await _dependencyResolver
                .Resolve<IQueryHandler<TQuery, TResult>>()
                .Handle(query, cancellationToken)
                .ConfigureAwait(false);
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

    public class QueryService2 //: IQueryService
    {
        private readonly Func<Type, object> _getHandlerInstanceFromType;
        private readonly Type _handlerInterfaceType = typeof(IQueryHandler<,>);

        public QueryService2(Func<Type, object> getHandlerInstanceFromType)
        {
            _getHandlerInstanceFromType = getHandlerInstanceFromType;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = new CancellationToken())
        {
            var handlerType = _handlerInterfaceType.MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _getHandlerInstanceFromType(handlerType);

            return await handler
                .Handle((dynamic)query, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public class InProcQueryService : IQueryService
    {
        private static readonly Type HandlerInterfaceType = typeof(IQueryHandler<,>);
        private readonly ConcurrentDictionary<Type, Lazy<HandlerInfo>> _cachedInfo = new ConcurrentDictionary<Type, Lazy<HandlerInfo>>();
        private readonly IDependencyResolver _dependencyResolver;

        public InProcQueryService(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var handlerInfo  = _cachedInfo.LazyGetOrAdd(query.GetType(), qt => new HandlerInfo(qt, typeof(TResult)));
            var queryHandler = _dependencyResolver.Resolve(handlerInfo.Type);
            var task         = (Task<TResult>)handlerInfo.Invoker(queryHandler, query, cancellationToken);

            return await task.ConfigureAwait(false);
        }

        private struct HandlerInfo
        {
            public Type Type { get; }
            public Func<object, object, CancellationToken, object> Invoker { get; }

            public HandlerInfo(Type queryType, Type resultType)
            {
                var handlerType = HandlerInterfaceType.MakeGenericType(queryType, resultType);
                var methodInfo  = handlerType.GetMethod("Handle");

                Type    = handlerType;
                Invoker = (h, q, ct) => methodInfo.Invoke(h, new[] { q, ct });
            }
        }
    }

    public class DynamicQueryServiceWithResolver : IQueryService
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly Type _handlerInterfaceType = typeof(IQueryHandler<,>);

        public DynamicQueryServiceWithResolver(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var handlerType = _handlerInterfaceType
                .MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = _dependencyResolver.Resolve(handlerType);

            return await handler.Handle((dynamic)query, cancellationToken).ConfigureAwait(false);
        }
    }

    public class DynamicQueryServiceWithoutResolver : IQueryService
    {
        private readonly Dictionary<Type, Func<object>> _handlerFactories;

        public DynamicQueryServiceWithoutResolver(Dictionary<Type, Func<object>> handlerFactories)
        {
            _handlerFactories = handlerFactories;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = new CancellationToken())
        {
            var queryType = query.GetType();

            Func<dynamic> handlerFactory;
            if(!_handlerFactories.TryGetValue(queryType, out handlerFactory)) {
                throw new ApplicationException($"Failed to find handler for query {queryType}");
            }

            return await handlerFactory()
                .Handle((dynamic)query, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    //public class QueryService : IQueryService
    //{
    //    private readonly Dictionary<Type, Func<object>> _handlerFactories = new Dictionary<Type, Func<object>>();

    //    public QueryService()
    //    { }

    //    public QueryService(Dictionary<Type, Func<object>> handlerFactories)
    //    {
    //        _handlerFactories = handlerFactories;
    //    }

    //    public QueryService(params KeyValuePair<Type, Func<object>>[] handlerFactories)
    //    {
    //        _handlerFactories = handlerFactories.ToDictionary(pair => pair.Key, pair => pair.Value);
    //    }

    //    public QueryService(params Tuple<Type, Func<object>>[] handlerFactories)
    //    {
    //        _handlerFactories = handlerFactories.ToDictionary(pair => pair.Item1, pair => pair.Item2);
    //    }

    //    public void RegisterHandler<TQueryHandler, TQuery, TResult>(Func<TQueryHandler> handlerFactory)
    //        where TQueryHandler : IQueryHandler<TQuery,TResult>
    //        where TQuery : IQuery<TResult>
    //    {
    //        var queryType = typeof(TQuery);

    //        if (_handlerFactories.ContainsKey(queryType)) {
    //            _handlerFactories[queryType] = handlerFactory as Func<object>;
    //        }
    //        else {
    //            _handlerFactories.Add(queryType, handlerFactory as Func<object>);
    //        }
    //    }

    //    public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = new CancellationToken())
    //    {
    //        var queryType = query.GetType();

    //        Func<dynamic> handlerFactory;
    //        if(!_handlerFactories.TryGetValue(queryType, out handlerFactory))
    //        {
    //            throw new ApplicationException($"Failed to find handler for query {queryType}");
    //        }

    //        return await handlerFactory()
    //            .Handle((dynamic)query, cancellationToken)
    //            .ConfigureAwait(false);
    //    }
    //}

    public class ReflectionQueryServiceWithResolver : IQueryService
    {
        private static readonly Type HandlerInterfaceType = typeof(IQueryHandler<,>);
        private readonly ConcurrentDictionary<Type, HandlerInfo> _cachedInfo = new ConcurrentDictionary<Type, HandlerInfo>();
        private readonly IDependencyResolver _dependencyResolver;

        public ReflectionQueryServiceWithResolver(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var handlerInfo  = _cachedInfo.GetOrAdd(query.GetType(), qt => new HandlerInfo(qt, typeof(TResult)));
            var queryHandler = _dependencyResolver.Resolve(handlerInfo.Type);
            var task         = (Task<TResult>)handlerInfo.MethodFunc(queryHandler, query, cancellationToken);

            return await task.ConfigureAwait(false);
        }

        private struct HandlerInfo
        {
            public Type Type { get; }
            public Func<object, object, CancellationToken, object> MethodFunc { get; }

            public HandlerInfo(Type queryType, Type resultType)
            {
                var handlerType = HandlerInterfaceType.MakeGenericType(queryType, resultType);
                var methodInfo  = handlerType.GetMethod("Run");

                Type       = handlerType;
                MethodFunc = (h, q, ct) => methodInfo.Invoke(h, new[] { q, ct });
            }
        }
    }

    public class ReflectionQueryServiceWithoutResolver : IQueryService
    {
        private static readonly Type HandlerInterfaceType = typeof(IQueryHandler<,>);
        private readonly ConcurrentDictionary<Type, HandlerInfo> _cachedInfo = new ConcurrentDictionary<Type, HandlerInfo>();
        private readonly Dictionary<Type, Func<object>> _handlerFactories;

        public ReflectionQueryServiceWithoutResolver(Dictionary<Type, Func<object>> handlerFactories)
        {
            _handlerFactories = handlerFactories;
        }

        public async Task<TResult> Run<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var handlerInfo = _cachedInfo.GetOrAdd(query.GetType(), qt => new HandlerInfo(qt, typeof(TResult)));

            Func<object> handlerFactory;
            if(!_handlerFactories.TryGetValue(handlerInfo.Type, out handlerFactory)) {
                throw new ApplicationException($"Failed to find handler for query {query.GetType()}");
            }

            var task = (Task<TResult>)handlerInfo.Invoker(handlerFactory(), query, cancellationToken);

            return await task.ConfigureAwait(false);
        }

        private struct HandlerInfo
        {
            public Type Type { get; }
            public Func<object, object, CancellationToken, object> Invoker { get; }

            public HandlerInfo(Type queryType, Type resultType)
            {
                var handlerType = HandlerInterfaceType.MakeGenericType(queryType, resultType);
                var methodInfo  = handlerType.GetMethod("Handle");

                Type    = handlerType;
                Invoker = (h, q, ct) => methodInfo.Invoke(h, new[] { q, ct });
            }
        }
    }
}