using System;
using System.Collections.Generic;
using Aenima.EventStore;
using Aenima.System;
using Aenima.System.Extensions;
using Newtonsoft.Json;

namespace Aenima.JsonNet
{
    public class JsonNetEventSerializer : IEventSerializer
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            TypeNameHandling  = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver  = EventContractResolver.Instance
        };

        public NewStreamEvent ToNewStreamEvent<TEvent>(TEvent e, IDictionary<string, object> metadata = null) where TEvent : class, IEvent
        {
            var jsonData     = JsonConvert.SerializeObject(e, Settings);
            var jsonMetadata = metadata != null 
                ? JsonConvert.SerializeObject(metadata, Settings)
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
                JsonConvert.DeserializeObject(
                    streamEvent.Metadata,
                    typeof(IDictionary<string, object>),
                    Settings) as IDictionary<string, object>;

            return JsonConvert.DeserializeObject(
                value   : streamEvent.Data, 
                type    : typeof(TEvent), 
                settings: Settings) as TEvent;
        }
    }

    //public class JsonNetEventSerializer : IEventSerializer
    //{
    //    public static readonly JsonSerializerSettings ToNewStreamEventSerializerSettings = new JsonSerializerSettings
    //    {
    //        TypeNameHandling = TypeNameHandling.None,
    //        NullValueHandling = NullValueHandling.Ignore,
    //        ContractResolver = ToNewStreamEventContractResolver.Instance
    //    };

    //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //    {
    //        TypeNameHandling = TypeNameHandling.None,
    //        NullValueHandling = NullValueHandling.Ignore,
    //        ContractResolver = EventContractResolver.Instance
    //    };

    //    public NewStreamEvent ToNewStreamEvent<TEvent>(TEvent e, IDictionary<string, object> metadata = null) where TEvent : class, IEvent
    //    {
    //        var jsonData = JsonConvert.SerializeObject(e, ToNewStreamEventSerializerSettings);
    //        var jsonMetadata = JsonConvert.SerializeObject(metadata, ToNewStreamEventSerializerSettings);

    //        return new NewStreamEvent((Guid)metadata["Id"], e.GetType().Name, jsonData, jsonMetadata);
    //    }

    //    public TEvent FromStreamEvent<TEvent>(StreamEvent streamEvent, out IDictionary<string, object> metadata) where TEvent : class, IEvent
    //    {
    //        metadata =
    //            JsonConvert.DeserializeObject(
    //                streamEvent.Data,
    //                typeof(IDictionary<string, object>),
    //                Settings) as IDictionary<string, object>;

    //        var eventClrType = metadata?["EventClrType"]?.ToString();

    //        if(eventClrType == null)
    //        {
    //            throw new MissingFieldException($"Failed to find event CLR Type for stream event with Type {streamEvent.Type}! Invalid metadata!");
    //        }

    //        return JsonConvert.DeserializeObject(
    //            value: streamEvent.Data,
    //            type: Type.GetType(eventClrType),
    //            settings: Settings) as TEvent;
    //    }
    //}
}
