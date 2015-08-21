using System.Collections.Generic;

namespace Aenima
{
    public interface IAggregate
    {
        string Id { get; }
        int Version { get; }
        IEnumerable<object> GetChanges();
        void AcceptChanges();
        void Restore(IState state);
    }
}