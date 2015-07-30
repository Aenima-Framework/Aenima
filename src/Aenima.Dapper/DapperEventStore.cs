using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Aenima.Dapper.Extensions;
using Aenima.EventStore;
using Aenima.Exceptions;
using Aenima.System;
using Dapper;

// ReSharper disable PossibleMultipleEnumeration

namespace Aenima.Dapper
{
    public sealed class DapperEventStore : IEventStore
    {
        //private read-only ILog log = Log.ForContext<DapperEventStore>();

        private readonly IEventSerializer serializer;
        private readonly IEventDispatcher dispatcher;
        private readonly DapperEventStoreSettings settings;

        public DapperEventStore(IEventSerializer serializer, IEventDispatcher dispatcher, DapperEventStoreSettings settings)
        {
            this.serializer = serializer;
            this.dispatcher = dispatcher;
            this.settings   = settings;
        }

        public async Task AppendStream(string streamId, int expectedVersion, IEnumerable<StreamEvent> streamEvents)
        {
            Guard.NullOrWhiteSpace(() => streamId);
            Guard.NullOrDefault(() => streamEvents);

            // create DataTable to send as a TVP
            var newStreamEventsTable = new DataTable();

            newStreamEventsTable.Columns.Add("Id", typeof(Guid));
            newStreamEventsTable.Columns.Add("Type", typeof(string));
            newStreamEventsTable.Columns.Add("Data", typeof(string));
            newStreamEventsTable.Columns.Add("Metadata", typeof(string));
            newStreamEventsTable.Columns.Add("StreamVersion", typeof(int));

            var eventVersion     = expectedVersion;
            var fallbackCommitId = Guid.NewGuid();

            newStreamEventsTable.BeginLoadData();

            foreach(var se in streamEvents) {
                eventVersion++;

                // the event store will always try to add this headers
                // which are mandatory for the correct behavior of the framework
                if(!se.Metadata.ContainsKey(EventMetadataKeys.Id)) {
                    se.Metadata[EventMetadataKeys.Id] = Guid.NewGuid();
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.ClrType)) {
                    se.Metadata[EventMetadataKeys.ClrType] = se.Event.GetType().AssemblyQualifiedName;
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.StreamId)) {
                    se.Metadata[EventMetadataKeys.StreamId] = streamId;
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.AggregateVersion)) {
                    se.Metadata[EventMetadataKeys.AggregateVersion] = eventVersion;
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.CommitId)) {
                    se.Metadata[EventMetadataKeys.CommitId] = fallbackCommitId;
                }

                // add the DataRow
                newStreamEventsTable.Rows.Add(
                    se.Metadata[EventMetadataKeys.Id],
                    se.Event.GetType().Name,
                    serializer.Serialize(se.Event),
                    serializer.Serialize(se.Metadata),
                    eventVersion);
            }

            newStreamEventsTable.EndLoadData();

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new {
                StreamId              = streamId,
                ExpectedStreamVersion = expectedVersion,
                StreamEvents          = newStreamEventsTable.AsTableValuedParameter("StreamEvents")
            });

            // execute operation
            await WithTransaction(
                async t => {
                    var actualVersion = await t.ExecuteScalar("AppendStream", parameters);

                    // if the actual version is different from the expected version
                    if(actualVersion != eventVersion) {
                        throw new StreamConcurrencyException(streamId, expectedVersion, actualVersion);
                    }
                },
                "AppendStream")
                .ConfigureAwait(false);

            // dispatch streamEvents
            await dispatcher.DispatchStreamEvents(streamEvents);
        }

        public class StreamEventData
        {
            public string Data { get; set; }

            public string Metadata { get; set; }
        }

        public async Task<StreamEventsPage> ReadStream(string streamId, int fromVersion, int count, StreamReadDirection direction = StreamReadDirection.Forward)
        {
            Guard.NullOrWhiteSpace(() => streamId);

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new {
                StreamId    = streamId,
                FromVersion = fromVersion,
                Count       = count,
                ReadForward = direction == StreamReadDirection.Forward ? 1 : 0
            });

            parameters.AddOutput("Error");
            parameters.AddReturnValue();

            IEnumerable<StreamEventData> result = null;

            // execute operation
            await WithTransaction(
                async t => {
                    result = await t.QueryProcedure<StreamEventData>("ReadStream", parameters);

                    // check for errors 
                    switch(parameters.GetOutput<int>("Error")) {
                        case -100:
                            throw new StreamNotFoundException(streamId);
                        case -200:
                            throw new StreamDeletedException(streamId, fromVersion);
                    }
                },
                "ReadStream")
                .ConfigureAwait(false);

            // return stream streamEvents
            var streamVersion = parameters.GetReturnValue();

            var streamEvents = result.Select(
                sed => {
                    var metadata        = serializer.Deserialize<IDictionary<string, object>>(sed.Metadata);
                    var domainEventType = Type.GetType(metadata[EventMetadataKeys.ClrType].ToString());
                    var streamEvent     = serializer.DeserializeAs<IEvent>(sed.Data, domainEventType);

                    return new StreamEvent(streamEvent, metadata);
                }).ToList();

            return new StreamEventsPage(
                streamId   : streamId,
                fromVersion: fromVersion,
                lastVersion: streamVersion,
                events     : streamEvents,
                direction  : direction);
        }

        public Task DeleteStream(string streamId, bool forever = false)
        {
            throw new NotImplementedException();
        }

        //private async Task<TResult> WithTransaction<TResult>(
        //    Func<SqlTransaction, Task<TResult>> operation, Action validate, string transactionName)
        //{
        //    using(var connection = new SqlConnection(this.settings.ConnectionString)) {
        //        try {
        //            await connection.OpenAsync().ConfigureAwait(false);
        //        }
        //        catch(Exception ex) {
        //            throw new StorageUnavailableException(ex);
        //        }

        //        using(var transaction = connection.BeginTransaction(IsolationLevel.Serializable, transactionName)) {
        //            try {
        //                var result = await operation(transaction);
        //                validate();
        //                transaction.Commit();
        //                return result;
        //            }
        //            catch(Exception ex) {
        //                transaction.Rollback();

        //                if(ex is StreamConcurrencyException
        //                    || ex is StreamDeletedException
        //                    || ex is StreamNotFoundException) {
        //                    throw;
        //                }

        //                throw new StorageException(ex);
        //            }
        //        }
        //    }
        //}

        private async Task WithTransaction(
           Func<SqlTransaction, Task> operation, string transactionName)
        {
            using(var connection = new SqlConnection(this.settings.ConnectionString)) {
                try {
                    await connection.OpenAsync();
                }
                catch(Exception ex) {
                    throw new StorageUnavailableException(ex);
                }

                using(var transaction = connection.BeginTransaction(IsolationLevel.Serializable, transactionName)) {
                    try {
                        await operation(transaction);
                        transaction.Commit();
                    }
                    catch(Exception ex) {
                        transaction.Rollback();

                        if(ex is StreamConcurrencyException
                        || ex is StreamDeletedException
                        || ex is StreamNotFoundException) {
                            throw;
                        }

                        throw new StorageException(ex);
                    }
                }
            }
        }
    }

    //public static class SerializerStreamEventDataExtensions
    //{
    //    public static StreamEventData ToStreamEventData(this IEventSerializer serializer, StreamEvent streamEvent)
    //    {
    //        var eventId = streamEvent.Metadata != null && streamEvent.Metadata.ContainsKey("Id")
    //           ? streamEvent.Metadata["Id"].ToGuid()
    //           : SequentialGuid.New();

    //        return new StreamEventData(
    //            id: eventId,
    //            type: streamEvent.Event.GetType().Name,
    //            data: serializer.Serialize(streamEvent.Event),
    //            metadata: serializer.Serialize(streamEvent.Metadata));
    //    }

    //    public static StreamEvent ToStreamEvent(this IEventSerializer serializer, StreamEventData streamEventData)
    //    {
    //        return new StreamEvent(
    //            serializer.Deserialize<IEvent>(streamEventData.Data),
    //            serializer.Deserialize<IDictionary<string, object>>(streamEventData.Metadata));
    //    }

    //}

    //public sealed class DapperEventStoreOld : IEventStore
    //{
        

    //    public async Task AppendStream(
    //        string streamId,
    //        int expectedVersion,
    //        IEnumerable<NewStreamEvent> streamEvents,
    //        CancellationToken cancellationToken = default(CancellationToken))
    //    {
    //        Guard.NullOrWhiteSpace(() => streamId);
    //        Guard.NullOrDefault(() => streamEvents);

    //        // create DataTable to send as a TVP
    //        var newStreamEventsTable = new DataTable();

    //        newStreamEventsTable.Columns.Add("Id", typeof(Guid));
    //        newStreamEventsTable.Columns.Add("Type", typeof(string));
    //        newStreamEventsTable.Columns.Add("Data", typeof(string));
    //        newStreamEventsTable.Columns.Add("Metadata", typeof(string));
    //        newStreamEventsTable.Columns.Add("StreamVersion", typeof(int));

    //        newStreamEventsTable.BeginLoadData();

    //        var eventVersion = expectedVersion == -1 ? 0 : expectedVersion;

    //        foreach(var e in streamEvents) {        
    //            newStreamEventsTable.LoadDataRow(
    //                new object[] {
    //                    e.Id, e.Type, e.Data, e.Metadata, eventVersion
    //                }, 
    //                LoadOption.Upsert);
    //            eventVersion++;
    //        }
    //        newStreamEventsTable.EndLoadData();

    //        // create parameters
    //        var parameters = new DynamicParameters();

    //        parameters.AddDynamicParams(new {
    //            StreamId              = streamId,
    //            StreamType            = "Not implemented yet.",
    //            ExpectedStreamVersion = expectedVersion,
    //            StreamEvents          = newStreamEventsTable.AsTableValuedParameter("StreamEvents")
    //        });

    //        // execute operation
    //        await WithTransaction(
    //            transactionName: "AppendStream",
    //            operation: async transaction => {
    //                try {
    //                    await transaction.ExecuteProcedure("AppendStream", parameters);
    //                }
    //                catch(SqlException ex) {
    //                    if(ex.Number == 50001) {
    //                        throw new StreamConcurrencyException(streamId, expectedVersion, GetActualVersionFromErrorMessage(ex.Message));
    //                    }

    //                    throw;
    //                }       
    //            });
    //    }

    //    private static int GetActualVersionFromErrorMessage(string errorMessage)
    //    {
    //        var separatorIndex = errorMessage.LastIndexOf(':');

    //        return int.Parse(errorMessage.Substring(separatorIndex));
    //    }

    //    public async Task<StreamEventsPage> ReadStream(
    //        string streamId,
    //        int fromVersion,
    //        int count,
    //        StreamReadDirection direction = StreamReadDirection.Forward,
    //        CancellationToken cancellationToken = default(CancellationToken))
    //    {
    //        Guard.NullOrWhiteSpace(() => streamId);

    //        // create parameters
    //        var parameters = new DynamicParameters();

    //        parameters.AddDynamicParams(new
    //        {
    //            StreamId    = streamId,
    //            FromVersion = fromVersion,
    //            Count       = count,
    //            ReadForward = direction == StreamReadDirection.Forward ? 1 : 0
    //        });

    //        parameters.AddReturnValue();

    //        IEnumerable<StreamEvent> result = new List<StreamEvent>();

    //        // execute operation
    //        await WithTransaction(
    //            transactionName: "ReadStream", 
    //            operation: async transaction => {
    //                try {
    //                    result = await transaction.QueryProcedure<StreamEvent>("ReadStream", parameters);
    //                }
    //                catch(SqlException ex) {
    //                    switch(ex.Number) {
    //                        case 50100:
    //                            throw new StreamNotFoundException(streamId);
    //                        case 50200:
    //                            throw new StreamDeletedException(streamId, fromVersion);
    //                        default:
    //                            throw;
    //                    }
    //                }
    //            });

    //        var status = parameters.GetReturnValue();

    //        return new StreamEventsPage(
    //            streamId,
    //            fromVersion,
    //            status,
    //            result,
    //            direction);
    //    }

    //    public async Task DeleteStream(
    //        string streamId,
    //        bool forever = false,
    //        CancellationToken cancellationToken = default(CancellationToken))
    //    {
    //        throw new NotImplementedException();

    //        Guard.NullOrWhiteSpace(() => streamId);

    //        // create parameters
    //        var parameters = new DynamicParameters();

    //        parameters.AddDynamicParams(new
    //        {
    //            StreamId = streamId,
    //            Forever  = forever
    //        });

    //        parameters.AddReturnValue();

    //        // execute operation
    //        await WithTransaction(
    //            transactionName: "DeleteStream",
    //            operation: async transaction => {
    //                await transaction.ExecuteProcedure("DeleteStream", parameters);
    //            });

    //        var status = parameters.GetReturnValue();

    //        switch(status) {
    //            case -100: throw new StreamNotFoundException(streamId);
    //        }
    //    }

    //    private async Task WithTransaction(
    //        Func<SqlTransaction, Task> operation,
    //        IsolationLevel isolationLevel = IsolationLevel.Serializable,
    //        string transactionName = null)
    //    {
    //        using(var connection = new SqlConnection(this.settings.ConnectionString)) {
    //            try {
    //                await connection.OpenAsync().ConfigureAwait(false);
    //            }
    //            catch(Exception ex) {
    //                throw new StorageUnavailableException(ex);
    //            }

    //            using(var transaction = connection.BeginTransaction(isolationLevel, transactionName)) {
    //                try {
    //                    await operation(transaction);
    //                    transaction.Commit();
    //                }
    //                catch(Exception ex) {
    //                    transaction.Rollback();

    //                    if(ex is StreamConcurrencyException 
    //                    || ex is StreamDeletedException 
    //                    || ex is StreamNotFoundException) {
    //                        throw;
    //                    }

    //                    throw new StorageException(ex);
    //                }
    //            }
    //        }
    //    }

    //}
}