using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace Aenima.EventStore
{
    public class StreamEventsPage
    {
        public readonly string StreamId;
        public readonly int FromVersion;
        public readonly int NextVersion;
        public readonly int LastVersion;
        public readonly bool IsEndOfStream;
        public readonly IReadOnlyCollection<StreamEvent> Events;
        public readonly StreamReadDirection Direction;

        public StreamEventsPage(
            string streamId,
            int fromVersion,
            int toVersion,
            int lastVersion,
            IEnumerable<StreamEvent> events,
            StreamReadDirection direction)
        {
            var readonlyEvents = events.ToList().AsReadOnly();
 
            IsEndOfStream = direction == StreamReadDirection.Forward
                ? toVersion == lastVersion
                : toVersion == 0;
            StreamId      = streamId;
            FromVersion   = fromVersion;
            NextVersion   = IsEndOfStream
                ? -1
                : direction == StreamReadDirection.Forward
                    ? toVersion + 1
                    : toVersion - 1;
            LastVersion   = lastVersion;
            Events        = readonlyEvents;
            Direction     = direction;
        }
    }
}