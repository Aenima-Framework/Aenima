using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Aenima.EventStore;
using Aenima.System;
using Aenima.System.Extensions;

namespace Aenima
{
    public static class AggregateFactory
    {
        public static TAggregate Create<TAggregate>(params object[] domainEvents)
            where TAggregate : class, IAggregate, new()
        {
            var aggregateRoot = new TAggregate();

            if (domainEvents != null) {
                aggregateRoot.Restore(domainEvents);
            }

            return aggregateRoot;
        }
    }


    //public class SnapshotRepository : IRepository
    //{
    //    private readonly ISerializer _serializer;

    //    public SnapshotRepository(ISerializer serializer)
    //    {
    //        _serializer = serializer;
    //    }

    //    public Task<TAggregate> GetById<TAggregate>(string identity, int version) where TAggregate : class, IAggregate, new()
    //    {
    //        Snapshot snapshot;

    //        var domainEventType = Type.GetType(metadata[EventMetadataKeys.ClrType].ToString());
    //        var streamEvent = _serializer.DeserializeAs<object>(snapshot.State, domainEventType);

    //        _serializer.Deserialize(snapshot.State,)

    //    }

    //    public Task Save<T>(T aggregate, IDictionary<string, string> headers = null) where T : class, IAggregate
    //    {
    //        Guard.NullOrDefault(() => aggregate);

    //        var commitId = Guid.NewGuid().ToString();

    //        var metadata = new Dictionary<string, string> {
    //            {EventMetadataKeys.Id         , Guid.NewGuid().ToString()},
    //            {EventMetadataKeys.ClrType    , typeof(T).AssemblyQualifiedName},
    //            {EventMetadataKeys.AggregateId, aggregate.Id},
    //            {EventMetadataKeys.Version    , (aggregate.Version + idx).ToString()},
    //            {EventMetadataKeys.CommitId   , commitId}
    //        };
    //        return new StreamEvent(domainEvent, eventMetadata.Merge(headers));

    //    }
    //}

    //public enum ReadDirection
    //{
    //    /// <summary>
    //    /// From beginning to end.
    //    /// </summary>
    //    Forward,
    //    /// <summary>
    //    /// From end to beginning.
    //    /// </summary>
    //    Backward
    //}

    //public interface ISnapshotStore
    //{
    //    Task AddSnapshot(
    //        string aggregateId,
    //        long expectedVersion,
    //        object state,
    //        IDictionary<string, string> metadata);

    //    Task UpdateSnapshot(
    //        string aggregateId,
    //        long expectedVersion,
    //        object state,
    //        IDictionary<string, string> metadata);

    //    Task<Snapshot> GetSnapshot(string aggregateId, long version);

    //    Task<Snapshot> GetLatestSnapshot(string aggregateId);

    //    Task<Snapshot> GetAllSnapshots(
    //        string aggregateId,
    //        long fromVersion,
    //        int count,
    //        ReadDirection direction);

    //    Task DeleteSnapshot(string aggregateId, long version);

    //    Task DeleteAllSnapshots(string aggregateId, long fromVersion, long toVersion);

    //    Task Initialize();
    //}

    //public static class SnapshotStoreExtensions
    //{
    //    //    public static Task AddSnapshot(this ISnapshotStore store, Snapshot snapshot)
    //    //    {
    //    //        return store.AddSnapshot(
    //    //            snapshot.AggregateState.Id, 
    //    //            snapshot.AggregateState.Version, 
    //    //            snapshot.Metadata);
    //    //    }
    //}

    //public class Snapshot
    //{
    //    public readonly object State;
    //    public readonly IDictionary<string, string> Metadata;

    //    public Snapshot(object state, IDictionary<string, string> metadata)
    //    {
    //        State = state;
    //        Metadata = metadata ?? new Dictionary<string, string>();
    //    }
    //}

}