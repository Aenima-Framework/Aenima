using System.Collections.Generic;

namespace Aenima.EventStore
{
    public class StreamEvent
    {
        public readonly IEvent Event;
        public readonly IDictionary<string, object> Metadata;

        public StreamEvent(IEvent e, IDictionary<string, object> metadata)
        {
            Event    = e;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
    }
}