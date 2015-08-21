using System.Collections.Generic;

namespace Aenima.EventStore
{
    public class StreamEvent
    {
        public readonly object Event;
        public readonly IDictionary<string, string> Metadata;

        public StreamEvent(object e, IDictionary<string, string> metadata)
        {
            Event    = e;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}