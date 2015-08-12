using System.Collections.Generic;

namespace Aenima.EventStore
{
    public class StreamEvent
    {
        public readonly IEvent Event;
        public readonly IDictionary<string, string> Metadata;

        public StreamEvent(IEvent e, IDictionary<string, string> metadata)
        {
            Event    = e;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}