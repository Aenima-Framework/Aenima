using System.Collections.Generic;
using Aenima.EventStore;

namespace Aenima
{
    public static class EventSerializerExtensions
    {
        public static IEvent FromStreamEvent<TEvent>(this IEventSerializer serializer, StreamEvent streamEvent)
            where TEvent : class, IEvent
        {
            IDictionary<string, object> metadata;
            return serializer.FromStreamEvent<TEvent>(streamEvent, out metadata);
        }
    }
}