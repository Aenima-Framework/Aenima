using System;

namespace Aenima.EventStore
{
    /// <summary>
    /// Represents an event in the event store.
    /// </summary>
    public class StreamEvent
    {
        public readonly Guid Id;
        public readonly string Type;
        public readonly string Data;
        public readonly string Metadata;
        public readonly DateTime StoredOn;
        public readonly string StreamId;
        public readonly int StreamVersion;

        public StreamEvent(
            Guid id,
            string type,
            string data,
            string metadata,
            DateTime storedOn,
            string streamId,
            int streamVersion)
        {
            Id            = id;
            Type          = type;
            Data          = data;
            Metadata      = metadata;
            StoredOn      = storedOn;
            StreamId      = streamId;
            StreamVersion = streamVersion;
        }
    }
}