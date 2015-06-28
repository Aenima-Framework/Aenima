using System.Collections.Generic;
using Aenima.EventStore;
using Jil;

namespace Aenima.Jil
{
    public class JilDomainEventSerializer : IDomainEventSerializer
    {
        private static readonly Options SerializationOptions = new Options(
            excludeNulls: true,
            unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsUTC);

        public NewStreamEvent ToNewStreamEvent(IDomainEvent ev, IDictionary<string, object> headers = null)
        {
            var data = JSON.Serialize(ev, SerializationOptions);

            var eventHeaders = new Dictionary<string, object>()
            {
                { "Id", ev.Id },
                { "AggregateId", ev.AggregateId },
                { "AggregateVersion", ev.AggregateVersion },
                { "RaisedOn", ev.RaisedOn },
                { "ProcessId", ev.ProcessId },
                {
                    "DomainEventClrType", ev.GetType()
                        .AssemblyQualifiedName
                }
            };

            if(headers != null) {
                foreach(var entry in headers) {
                    eventHeaders[entry.Key] = entry.Value;
                }
            }

            var metadata = JSON.Serialize(eventHeaders, SerializationOptions);

            return new NewStreamEvent(
                ev.Id,
                ev.GetType()
                    .Name,
                data,
                metadata);
        }

        public IDomainEvent FromStreamEvent(StreamEvent streamEvent)
        {
            var data = (streamEvent.Data + streamEvent.Metadata).Replace("}{", string.Empty);

            return JSON.DeserializeDynamic(data) as IDomainEvent;
        }
    }
}