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

                // the event store will always try to enrich the metadata since
                // some headers are mandatory.
                if(!se.Metadata.ContainsKey(EventMetadataKeys.Id)) {
                    se.Metadata[EventMetadataKeys.Id] = Guid.NewGuid();
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.ClrType)) {
                    se.Metadata[EventMetadataKeys.ClrType] = se.Event.GetType().AssemblyQualifiedName;
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.StreamId)) {
                    se.Metadata[EventMetadataKeys.StreamId] = streamId;
                }

                if(!se.Metadata.ContainsKey(EventMetadataKeys.Version)) {
                    se.Metadata[EventMetadataKeys.Version] = eventVersion;
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

                    // dispatch events
                    await dispatcher.DispatchStreamEvents(streamEvents);
                },
                "AppendStream")
                .ConfigureAwait(false);
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

            // return stream page
            var streamVersion = parameters.GetReturnValue();

            var streamEvents = result.Select(
                sed => {
                    var metadata        = serializer.Deserialize<IDictionary<string, object>>(sed.Metadata);
                    var domainEventType = Type.GetType(metadata[EventMetadataKeys.ClrType].ToString());
                    var streamEvent     = serializer.DeserializeAs<IEvent>(sed.Data, domainEventType);

                    return new StreamEvent(streamEvent, metadata);
                }).ToList();

            var lastReadEventVersion = int.Parse(
                streamEvents.Last().Metadata[EventMetadataKeys.Version].ToString());

            return new StreamEventsPage(
                streamId    : streamId,
                fromVersion : fromVersion,
                toVersion   : lastReadEventVersion,
                lastVersion : streamVersion,           
                events      : streamEvents,
                direction   : direction);
        }

        public Task DeleteStream(string streamId, bool forever = false)
        {
            throw new NotImplementedException();
        }

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
}