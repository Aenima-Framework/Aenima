using System.Collections.Generic;
using System.Threading.Tasks;
using Aenima.EventStore;

namespace Aenima
{
    public static class EventDispatcherExtensions
    {
        public static Task DispatchStreamEvent(this IDomainEventDispatcher dispatcher, StreamEvent streamEvent)
        {
            return dispatcher.Dispatch(streamEvent.Event, streamEvent.Metadata);
        }

        public static async Task DispatchStreamEvents(this IDomainEventDispatcher dispatcher, IEnumerable<StreamEvent> streamEvents)
        {
            foreach(var streamEvent in streamEvents) {
                await dispatcher.DispatchStreamEvent(streamEvent);
            }
        }

        public static async Task DispatchStreamEvents(this IDomainEventDispatcher dispatcher, params StreamEvent[] streamEvents)
        {
            foreach(var streamEvent in streamEvents)
            {
                await dispatcher.DispatchStreamEvent(streamEvent);
            }
        }
    }
}