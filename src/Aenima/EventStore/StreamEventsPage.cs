using System.Collections.Generic;
using System.Linq;

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

        private StreamEventsPage(
            string streamId,
            int fromVersion,
            int nextVersion,
            int lastVersion,
            bool isEndOfStream,
            IReadOnlyCollection<StreamEvent> events,
            StreamReadDirection direction)
        {
            StreamId      = streamId;
            FromVersion   = fromVersion;
            NextVersion   = nextVersion;
            LastVersion   = lastVersion;
            IsEndOfStream = isEndOfStream;
            Events        = events;
            Direction     = direction;
        }

        public StreamEventsPage(
           string streamId,
           int fromVersion,
           int lastVersion,
           IEnumerable<StreamEvent> events,
           StreamReadDirection direction)
        {
            var readonlyEvents = events.ToList().AsReadOnly();

            StreamId = streamId;
            FromVersion = fromVersion;
            NextVersion = direction == StreamReadDirection.Forward
                ? readonlyEvents.Last().StreamVersion + 1
                : readonlyEvents.Last().StreamVersion - 1;
            LastVersion = lastVersion;
            IsEndOfStream = direction == StreamReadDirection.Forward
                ? readonlyEvents.Last().StreamVersion == lastVersion
                : readonlyEvents.Last().StreamVersion == 0;
            Events = readonlyEvents;
            Direction = direction;
        }

        public static StreamEventsPage Create(
            string streamId,
            int fromVersion,
            int lastVersion,
            IEnumerable<StreamEvent> events,
            StreamReadDirection direction)
        {
            var readonlyEvents = events.ToList().AsReadOnly();

            return new StreamEventsPage(
                streamId,
                fromVersion,
                direction == StreamReadDirection.Forward
                    ? readonlyEvents.Last().StreamVersion + 1
                    : readonlyEvents.Last().StreamVersion - 1,
                lastVersion,
                direction == StreamReadDirection.Forward
                    ? readonlyEvents.Last().StreamVersion == lastVersion
                    : readonlyEvents.Last().StreamVersion == 0,
                readonlyEvents,
                direction);
        }
    }
}