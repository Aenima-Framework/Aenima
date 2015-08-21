using System.Collections.Generic;

namespace Aenima.EventStore
{
    public class StreamEvent
    {
        public readonly object Event;
        public readonly IDictionary<string, string> Metadata;

        public StreamEvent(object domainEvent, IDictionary<string, string> metadata)
        {
            Event    = domainEvent;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}