using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aenima.DependencyResolution;
using Aenima.System.Extensions;

namespace Aenima.Messaging
{
    public class InProcMessageBus : IMessageBus
    {
        private static readonly Type HandlerInterfaceType = typeof(IMessageHandler<>);
        private readonly IDependencyResolver _dependencyResolver;

        public InProcMessageBus(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public async Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken) where TEvent : class
        {
            var handlerType = HandlerInterfaceType.MakeGenericType(message.GetType());

            var handlers    = _dependencyResolver
                .ResolveAll(handlerType)
                .Cast<IMessageHandler<TEvent>>();

            foreach (var handler in handlers) {
                await handler
                    .Handle(message, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task Send<TCommand>(TCommand message, CancellationToken cancellationToken) where TCommand : class
        {
            var handler = _dependencyResolver.Resolve<IMessageHandler<TCommand>>();

            await handler
                .Handle(message, cancellationToken)
                .ConfigureAwait(false);
        }

        public void Dispose() {}
    }

    public class InProcMessageBusWithoutResolver : IMessageBus
    {
        private static readonly Type HandlerInterfaceType = typeof(IMessageHandler<>);
        private readonly Func<object>[] _handlers;

        
        public InProcMessageBusWithoutResolver(Func<object>[] handlers)
        {
            _handlers = handlers;
        }

        public async Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken) where TEvent : class
        {
            var handlerType = HandlerInterfaceType.MakeGenericType(message.GetType());

            var handlers = _handlers
                .Where(func => func.Target.GetType().IsAssignableFrom(handlerType));

            foreach(var handler in handlers) {
                await handler()
                    .As<IMessageHandler<TEvent>>()
                    .Handle(message, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task Send<TCommand>(TCommand message, CancellationToken cancellationToken) where TCommand : class
        {
            var handlerType = HandlerInterfaceType.MakeGenericType(message.GetType());

            var handler = _handlers
                .Single(func => func.Target.GetType().IsAssignableFrom(handlerType));

            await handler()
                .As<IMessageHandler<TCommand>>()
                .Handle(message, cancellationToken)
                .ConfigureAwait(false);            
        }

        public void Dispose() { }
    }
}