using System.Collections.Generic;
using System.Threading.Tasks;
using Aenima.EventStore;

namespace Aenima
{
    public static class EventDispatcherExtensions
    {
        public static Task DispatchStreamEvent(this IEventDispatcher dispatcher, StreamEvent streamEvent)
        {
            return dispatcher.Dispatch(streamEvent.Event, streamEvent.Metadata);
        }

        public static async Task DispatchStreamEvents(this IEventDispatcher dispatcher, IEnumerable<StreamEvent> streamEvents)
        {
            foreach(var streamEvent in streamEvents) {
                await dispatcher.DispatchStreamEvent(streamEvent);
            }
        }

        public static async Task DispatchStreamEvents(this IEventDispatcher dispatcher, params StreamEvent[] streamEvents)
        {
            foreach(var streamEvent in streamEvents)
            {
                await dispatcher.DispatchStreamEvent(streamEvent);
            }
        }
    }
}