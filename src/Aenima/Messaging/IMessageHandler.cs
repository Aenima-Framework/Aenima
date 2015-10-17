using System.Threading;
using System.Threading.Tasks;

namespace Aenima.Messaging
{
    public interface IMessageHandler<in TMessage>
    {
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}