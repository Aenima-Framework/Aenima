using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aenima.EventStore
{
    public interface IEventStore
    {
        Task AppendStream(
            string streamId,
            int expectedVersion,
            IEnumerable<StreamEvent> streamEvents);

        Task<StreamEventsPage> ReadStream(
            string streamId,
            int fromVersion,
            int count,
            StreamReadDirection direction = StreamReadDirection.Forward);

        Task DeleteStream(
            string streamId,
            bool forever = false);
    }

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

    //public class StreamEvent
    //{
    //    public readonly IEvent Event;
    //    public readonly IReadOnlyDictionary<string, object> Metadata;

    //    public StreamEvent(IEvent e, IDictionary<string, object> metadata)
    //    {
    //        Event = e;
    //        Metadata = metadata == null
    //            ? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())
    //            : new ReadOnlyDictionary<string, object>(metadata);
    //    }
    //}

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
            int lastVersion,
            IEnumerable<StreamEvent> events,
            StreamReadDirection direction)
        {
            var readonlyEvents = events.ToList().AsReadOnly();

            var lastEventVersion = int.Parse(
                readonlyEvents.Last().Metadata[EventMetadataKeys.AggregateVersion].ToString());

            IsEndOfStream = direction == StreamReadDirection.Forward
                ? lastEventVersion  == lastVersion
                : lastEventVersion  == 0;
            StreamId      = streamId;
            FromVersion   = fromVersion;
            NextVersion   = IsEndOfStream
                ? -1
                : direction == StreamReadDirection.Forward
                    ? lastEventVersion + 1
                    : lastEventVersion - 1;
            LastVersion   = lastVersion;
            Events        = readonlyEvents;
            Direction     = direction;
        }
    }
}