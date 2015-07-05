using System.Collections.Generic;
using Aenima.EventStore;
using Aenima.System;
using Aenima.System.Extensions;
using Jil;

namespace Aenima.Jil
{
    public class JilEventSerializer : IEventSerializer
    {
        private static readonly Options SerializationOptions = new Options(
            excludeNulls: true,
            unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsUTC);

        public NewStreamEvent ToNewStreamEvent<TEvent>(TEvent e, IDictionary<string, object> metadata = null) where TEvent : class, IEvent
        {
            var jsonData = JSON.Serialize(e, SerializationOptions);
            var jsonMetadata = metadata != null
                ? JSON.Serialize(metadata, SerializationOptions)
                : string.Empty;

            var eventId = metadata != null && metadata.ContainsKey("Id")
               ? metadata["Id"].ToGuidOrDefault(SequentialGuid.New())
               : SequentialGuid.New();

            return new NewStreamEvent(
                id      : eventId,
                type    : e.GetType().Name,
                data    : jsonData,
                metadata: jsonMetadata);
        }

        public TEvent FromStreamEvent<TEvent>(StreamEvent streamEvent, out IDictionary<string, object> metadata) where TEvent : class, IEvent
        {
            metadata =
                JSON.Deserialize<IDictionary<string, object>>(
                    streamEvent.Metadata,
                    SerializationOptions);

            return JSON.Deserialize<TEvent>(streamEvent.Data, SerializationOptions);
        }
    }
}