using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aenima.Messaging
{
    public interface IMessageBus : IDisposable
    {
        Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken) where TEvent : class;

        Task Send<TCommand>(TCommand message, CancellationToken cancellationToken) where TCommand : class;
    }
}