using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aenima.EventStore;

namespace Aenima.RavenDB
{
    public class RavenDbEventStore : IEventStore
    {
        public Task AppendStream(string streamId, int expectedVersion, IEnumerable<StreamEvent> streamEvents)
        {
            throw new NotImplementedException();
        }

        public Task<StreamEventsPage> ReadStream(string streamId, int fromVersion, int count, StreamReadDirection direction = StreamReadDirection.Forward)
        {
            throw new NotImplementedException();
        }

        public Task DeleteStream(string streamId, bool forever = false)
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
