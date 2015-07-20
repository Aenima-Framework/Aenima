using System.Collections.Generic;
using Aenima.EventStore;

namespace Aenima
{
    public interface IEventSerializer
    {
        NewStreamEvent ToNewStreamEvent<TEvent>(TEvent e, IDictionary<string, object> headers = null) where TEvent : class, IEvent;
        TEvent FromStreamEvent<TEvent>(StreamEvent streamEvent, out IDictionary<string, object> headers) where TEvent : class, IEvent;
    }
}