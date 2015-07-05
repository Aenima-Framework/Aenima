using System.Collections.Generic;
using Aenima.EventStore;

namespace Aenima
{
    public interface IEventSerializer
    {
        NewStreamEvent ToNewStreamEvent<TEvent>(TEvent e, IDictionary<string, object> metadata = null) where TEvent : class, IEvent;
        TEvent FromStreamEvent<TEvent>(StreamEvent streamEvent, out IDictionary<string, object> metadata) where TEvent : class, IEvent;

        //NewStreamEvent Serialize<TEvent>(TEvent e, IDictionary<string, object> metadata = null) where TEvent : class, IEvent;
        //TEvent DeseriLize<TEvent>(StreamEvent streamEvent, out IDictionary<string, object> metadata) where TEvent : class, IEvent;
    }
}