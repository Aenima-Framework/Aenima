using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aenima.DependencyResolution;

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

            var handlers = _dependencyResolver
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
}