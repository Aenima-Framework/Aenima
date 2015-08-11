using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aenima.EventStore
{
    public interface IEventStore
    {
        Task AppendStream(
            string streamId,
            int expectedVersion,
            IEnumerable<StreamEvent> streamEvents);

        Task<StreamEventsPage> ReadStream(
            string streamId,
            int fromVersion,
            int count,
            StreamReadDirection direction = StreamReadDirection.Forward);

        Task DeleteStream(
            string streamId,
            bool forever = false);

        Task Initialize();
    }
}