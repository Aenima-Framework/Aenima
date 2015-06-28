using System.Collections.Generic;
using Aenima.EventStore;

namespace Aenima
{
    public interface IDomainEventSerializer
    {
        NewStreamEvent ToNewStreamEvent(IDomainEvent ev, IDictionary<string, object> headers = null);
        IDomainEvent FromStreamEvent(StreamEvent streamEvent);
    }
}