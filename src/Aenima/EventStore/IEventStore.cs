using System;
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
            IEnumerable<NewStreamEvent> events,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<StreamEventsPage> ReadStream(
            string streamId,
            int fromVersion,
            int count,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteStream(
            string streamId,
            bool forever = false,
            CancellationToken cancellationToken = default(CancellationToken));
    }


    //public static class EventStoreExtensions
    //{
    //    public static Task AppendStream(
    //        this IEventStore eventStore,
    //        string streamId,
    //        int expectedVersion,
    //        IEnumerable<MetaEvent> events,
    //        CancellationToken cancellationToken = default(CancellationToken))
    //    {
            
    //    }

    //    public static Task<MetaStreamEventsPage> ReadStream(
    //       string streamId,
    //       int fromVersion,
    //       int count,
    //       StreamReadDirection direction = StreamReadDirection.Forward,
    //       CancellationToken cancellationToken = default(CancellationToken));
    //}

    //public class MetaEvent
    //{
    //    public readonly IEvent Event;
    //    public readonly IReadOnlyDictionary<string, object> Headers;

    //    public MetaEvent(IEvent e, IDictionary<string, object> headers)
    //    {
    //        Event   = e;
    //        Headers = new ReadOnlyDictionary<string, object>(headers);
    //    }
    //}

    //public class MetaStreamEventsPage
    //{
    //    public readonly string StreamId;
    //    public readonly int FromVersion;
    //    public readonly int NextVersion;
    //    public readonly int LastVersion;
    //    public readonly bool IsEndOfStream;
    //    public readonly IReadOnlyCollection<MetaEvent> Events;
    //    public readonly StreamReadDirection Direction;

    //    private MetaStreamEventsPage(
    //        string streamId,
    //        int fromVersion,
    //        int nextVersion,
    //        int lastVersion,
    //        bool isEndOfStream,
    //        IReadOnlyCollection<MetaEvent> events,
    //        StreamReadDirection direction)
    //    {
    //        StreamId      = streamId;
    //        FromVersion   = fromVersion;
    //        NextVersion   = nextVersion;
    //        LastVersion   = lastVersion;
    //        IsEndOfStream = isEndOfStream;
    //        Events        = events;
    //        Direction     = direction;
    //    }

    //    public static MetaStreamEventsPage Create(
    //        string streamId,
    //        int fromVersion,
    //        int nextVersion,
    //        int lastVersion,
    //        bool isEndOfStream,
    //        IEnumerable<MetaEvent> events,
    //        StreamReadDirection direction)
    //    {
    //        var readonlyEvents = events.ToList().AsReadOnly();

    //        return new MetaStreamEventsPage(
    //            streamId,
    //            fromVersion,
    //            nextVersion,
    //            lastVersion,
    //            isEndOfStream,
    //            readonlyEvents,
    //            direction);
    //    }
    //}
}