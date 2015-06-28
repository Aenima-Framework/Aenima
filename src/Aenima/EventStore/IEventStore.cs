using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aenima.EventStore
{
    public interface IEventStore
    {
        Task AppendStream(
            string streamId,
            int expectedVersion,
            IEnumerable<NewStreamEvent> events,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<StreamEventsPage> ReadStream(
            string streamId,
            int fromVersion,
            int count,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteStream(
            string streamId,
            bool forever = false,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}