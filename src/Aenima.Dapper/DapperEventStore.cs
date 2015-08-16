using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aenima.Dapper.Extensions;
using Aenima.EventStore;
using Aenima.Exceptions;
using Aenima.Logging;
using Aenima.System;
using Dapper;
using static System.String;

// ReSharper disable PossibleMultipleEnumeration

namespace Aenima.Dapper
{
    public static class StreamEventExtensions
    {
        /// <summary>
        /// The event store will always try to enrich the metadata since
        /// some headers are mandatory.
        /// </summary>
        public static void EnrichMetadata(this StreamEvent streamEvent, string streamId, int eventVersion, Guid fallbackCommitId)
        {
            if(!streamEvent.Metadata.ContainsKey(EventMetadataKeys.Id)) {
                streamEvent.Metadata[EventMetadataKeys.Id] = SequentialGuid.ForSqlServer.ToString();
            }

            if(!streamEvent.Metadata.ContainsKey(EventMetadataKeys.ClrType)) {
                streamEvent.Metadata[EventMetadataKeys.ClrType] = streamEvent.Event.GetType().AssemblyQualifiedName;
            }

            if(!streamEvent.Metadata.ContainsKey(EventMetadataKeys.StreamId)) {
                streamEvent.Metadata[EventMetadataKeys.StreamId] = streamId;
            }

            if(!streamEvent.Metadata.ContainsKey(EventMetadataKeys.Version)) {
                streamEvent.Metadata[EventMetadataKeys.Version] = eventVersion.ToString();
            }

            if(!streamEvent.Metadata.ContainsKey(EventMetadataKeys.CommitId)) {
                streamEvent.Metadata[EventMetadataKeys.CommitId] = fallbackCommitId.ToString();
            }
        }
    }

    public sealed class DapperEventStore : IEventStore
    {
        private readonly ILog _log = Log.ForContext<DapperEventStore>();

        private readonly ISerializer _serializer;
        private readonly IEventDispatcher _dispatcher;
        private readonly DapperEventStoreSettings _settings;

        public DapperEventStore(ISerializer serializer, IEventDispatcher dispatcher, DapperEventStoreSettings settings)
        {
            _serializer = serializer;
            _dispatcher = dispatcher;
            _settings   = settings;
        }

        public async Task Initialize()
        {
            await _log.Debug("Initializing Event Store");

            string script;
            using(var stream = Assembly
                .GetAssembly(typeof(DapperEventStore))
                .GetManifestResourceStream("Aenima.Dapper.Scripts.CreateStore.sql")) {
                if(stream == null) {
                    throw new Exception("Failed to find embedded script resource to initialize store!");
                }

                using(var reader = new StreamReader(stream))
                    script = reader.ReadToEnd();
            }

            using(var connection = new SqlConnection(_settings.ConnectionString)) {
                await connection
                    .ExecuteAsync(script)
                    .ConfigureAwait(false);
            }

            await _log.Debug("Event Store initialized");
        }

        public async Task AppendStream(string streamId, int expectedVersion, IEnumerable<StreamEvent> streamEvents)
        {
            Guard.NullOrWhiteSpace(() => streamId);
            Guard.NullOrDefault(() => streamEvents);

            await _log.Debug("Appending stream {@streamId} with {@events}", streamId, streamEvents.ToArray());

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
                se.EnrichMetadata(streamId, eventVersion++, fallbackCommitId);

                newStreamEventsTable.Rows.Add(
                    se.Metadata[EventMetadataKeys.Id],
                    se.Event.GetType().Name,
                    _serializer.Serialize(se.Event),
                    _serializer.Serialize(se.Metadata),
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

            int actualVersion;

            // execute operation
            using(var connection = new SqlConnection(_settings.ConnectionString)) {
                actualVersion = await connection
                    .ExecuteScalarAsync<int>(
                        sql        : "AppendStream", 
                        param      : parameters, 
                        commandType: CommandType.StoredProcedure)
                    .ConfigureAwait(false);
            }

            // if the actual version is different from the expected version
            if(actualVersion != eventVersion) {
                throw new StreamConcurrencyException(streamId, expectedVersion, actualVersion);
            }

            await _log.Information("Events appended to stream {@streamId}", streamId);

            // dispatch events
            await _dispatcher.DispatchStreamEvents(streamEvents);
        }
        
        public async Task<StreamEventsPage> ReadStream(string streamId, int fromVersion, int count, StreamReadDirection direction = StreamReadDirection.Forward)
        {
            Guard.NullOrWhiteSpace(() => streamId);

            await _log.Debug("Reading stream {@streamId} from version {fromVersion} to version {count}", streamId, fromVersion, count);

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

            IEnumerable<StreamEventData> result;

            // execute operation
            using(var connection = new SqlConnection(_settings.ConnectionString)) {
                // found a hardcore bug with Dapper!
                // if we exit a query without executing a select, the .QueryAsync<T> fails because it
                // tries to read the resultsetschema and fails. 
                // therefore we do not have access to return or output values.
                result = await connection
                    .QueryAsync<StreamEventData>(
                        sql        : "ReadStream",
                        param      : parameters,
                        commandType: CommandType.StoredProcedure)
                    .ConfigureAwait(false);
            }

            // check for errors 
            switch(parameters.GetOutput<int>("Error")) {
                case -100:
                    throw new StreamNotFoundException(streamId);
                case -200:
                    throw new StreamDeletedException(streamId, fromVersion);
            }

            await _log.Information("Stream {@streamId} read from version {fromVersion} to version {count}", streamId, fromVersion, count);

            // return stream page
            var streamVersion = parameters.GetReturnValue();

            var streamEvents = result.Select(
                streamEventData => {
                    var metadata        = _serializer.Deserialize<IDictionary<string, string>>(streamEventData.Metadata);
                    var domainEventType = Type.GetType(metadata[EventMetadataKeys.ClrType].ToString());
                    var streamEvent     = _serializer.DeserializeAs<IEvent>(streamEventData.Data, domainEventType);
                    return new StreamEvent(streamEvent, metadata);
                }).ToList();

            var lastReadEventVersion = streamEvents.Any()
                ? int.Parse(streamEvents.Last().Metadata[EventMetadataKeys.Version])
                : -1;

            await _log.Debug("Stream {@streamId} event serialization finished", streamId);

            return new StreamEventsPage(
                streamId   : streamId,
                fromVersion: fromVersion,
                toVersion  : lastReadEventVersion,
                lastVersion: streamVersion,           
                events     : streamEvents,
                direction  : direction);
        }

        public Task DeleteStream(string streamId, bool forever = false)
        {
            throw new NotImplementedException();
        }
    }
}