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
        public readonly DateTime CreatedOn;
        public readonly string StreamId;
        public readonly int StreamVersion;

        public StreamEvent(
            Guid id,
            string type,
            string data,
            string metadata,
            DateTime createdOn,
            string streamId,
            int streamVersion)
        {
            Id            = id;
            Type          = type;
            Data          = data;
            Metadata      = metadata;
            CreatedOn     = createdOn;
            StreamId      = streamId;
            StreamVersion = streamVersion;
        }
    }
}