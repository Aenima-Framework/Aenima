using System;
using System.Collections.Generic;
using Aenima.EventStore;
using Aenima.System.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aenima.JsonNet
{
    public class JsonNetDomainEventSerializer : IDomainEventSerializer
    {
        public static readonly JsonSerializerSettings ToNewStreamEventSerializerSettings = new JsonSerializerSettings {
            TypeNameHandling  = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver  = ToNewStreamEventContractResolver.Instance
        };

        public static readonly JsonSerializerSettings ToDomainEventSerializerSettings = new JsonSerializerSettings {
            TypeNameHandling  = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver  = ToDomainEventContractResolver.Instance
        };

        public NewStreamEvent ToNewStreamEvent(IDomainEvent ev, IDictionary<string, object> headers = null)
        {
            var data = JsonConvert.SerializeObject(ev, ToNewStreamEventSerializerSettings);

            var eventClrType = ev.GetType();

            var defaultHeaders = new Dictionary<string, object> {
                { "Id"                , ev.Id },        
                { "AggregateId"       , ev.AggregateId },
                { "AggregateVersion"  , ev.AggregateVersion },
                { "RaisedOn"          , ev.RaisedOn },
                { "ProcessId"         , ev.ProcessId },
                { "DomainEventClrType", eventClrType }
            };

            var metadata = JsonConvert.SerializeObject(defaultHeaders.Merge(headers), ToNewStreamEventSerializerSettings);

            return new NewStreamEvent(ev.Id, eventClrType.Name, data, metadata);
        }

        public IDomainEvent FromStreamEvent(StreamEvent streamEvent)
        {
            var data    = JObject.Parse(streamEvent.Data);
            var headers = JObject.Parse(streamEvent.Metadata);

            data.Merge(headers);

            var eventClrType = Type.GetType(headers.GetValue("DomainEventClrType").ToString());

            return JsonConvert.DeserializeObject(data.ToString(), eventClrType, ToDomainEventSerializerSettings) as IDomainEvent;
        }
    }
}
