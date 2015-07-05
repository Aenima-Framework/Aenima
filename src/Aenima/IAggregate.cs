using System.Collections.Generic;

namespace Aenima
{
    public interface IAggregate
    {
        string Id { get; }
        int Version { get; }
        IEnumerable<IEvent> GetChanges();
        void AcceptChanges();
        void Restore(IState state);
    }
}