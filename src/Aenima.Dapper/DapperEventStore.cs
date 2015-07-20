using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Aenima.Dapper.Extensions;
using Aenima.EventStore;
using Aenima.Exceptions;
using Aenima.System;
using Dapper;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PossibleMultipleEnumeration

namespace Aenima.Dapper
{
    public sealed class DapperEventStore : IEventStore
    {
        private readonly DapperEventStoreSettings settings;

        public DapperEventStore(DapperEventStoreSettings settings)
        {
            this.settings = settings;
        }

        public async Task AppendStream(
            string streamId,
            int expectedVersion,
            IEnumerable<NewStreamEvent> events,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.NullOrWhiteSpace(() => streamId);
            Guard.NullOrDefault(() => events);

            // create DataTable to send as TVP
            var newStreamEventsTable = new DataTable();

            newStreamEventsTable.Columns.Add("Id", typeof(Guid));
            newStreamEventsTable.Columns.Add("Type", typeof(string));
            newStreamEventsTable.Columns.Add("Data", typeof(string));
            newStreamEventsTable.Columns.Add("Metadata", typeof(string));
            newStreamEventsTable.Columns.Add("StreamVersion", typeof(int));

            newStreamEventsTable.BeginLoadData();
            foreach(var e in events) {
                newStreamEventsTable.LoadDataRow(
                    new object[] {
                        e.Id, e.Type, e.Data, e.Metadata, expectedVersion++
                    }, 
                    LoadOption.Upsert);
            }
            newStreamEventsTable.EndLoadData();

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new {
                StreamId              = streamId,
                StreamType            = "Not implemented yet.",
                ExpectedStreamVersion = expectedVersion,
                StreamEvents          = newStreamEventsTable.AsTableValuedParameter("StreamEvents")
            });

            // execute operation
            await WithTransaction(
                transactionName: "AppendStream",
                operation: async transaction => {
                    try {
                        await transaction.ExecuteProcedure("AppendStream", parameters);
                    }
                    catch(SqlException ex) {
                        if(ex.Number == 50001) {
                            throw new StreamConcurrencyException(streamId, expectedVersion, GetActualVersionFromErrorMessage(ex.Message));
                        }

                        throw;
                    }       
                });
        }

        private int GetActualVersionFromErrorMessage(string errorMessage)
        {
            var separatorIndex = errorMessage.IndexOf(":", StringComparison.Ordinal);

            return int.Parse(errorMessage.Substring(separatorIndex));
        }

        public async Task<StreamEventsPage> ReadStream(
            string streamId,
            int fromVersion,
            int count,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.NullOrWhiteSpace(() => streamId);

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new
            {
                StreamId    = streamId,
                FromVersion = fromVersion,
                Count       = count,
                ReadForward = direction == StreamReadDirection.Forward ? 1 : 0
            });

            parameters.AddReturnValue();

            IEnumerable<StreamEvent> result = new List<StreamEvent>();

            // execute operation
            await WithTransaction(
                transactionName: "ReadStream", 
                operation: async transaction => {
                    try {
                        result = await transaction.QueryProcedure<StreamEvent>("ReadStream", parameters);
                    }
                    catch(SqlException ex) {
                        switch(ex.Number) {
                            case 50100:
                                throw new StreamNotFoundException(streamId);
                            case 50200:
                                throw new StreamDeletedException(streamId, fromVersion);
                        }
                        throw;
                    }
                });

            var status = parameters.GetReturnValue();

            return new StreamEventsPage(
                streamId,
                fromVersion,
                status,
                result,
                direction);
        }

        public async Task DeleteStream(
            string streamId,
            bool forever = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();

            Guard.NullOrWhiteSpace(() => streamId);

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new
            {
                StreamId = streamId,
                Forever  = forever
            });

            parameters.AddReturnValue();

            // execute operation
            await WithTransaction(
                transactionName: "DeleteStream",
                operation: async transaction => {
                    await transaction.ExecuteProcedure("DeleteStream", parameters);
                });

            var status = parameters.GetReturnValue();

            switch(status) {
                case -100: throw new StreamNotFoundException(streamId);
            }
        }

        private async Task WithTransaction(
            Func<SqlTransaction, Task> operation,
            IsolationLevel isolationLevel = IsolationLevel.Serializable,
            string transactionName = null)
        {
            using(var connection = new SqlConnection(this.settings.ConnectionString)) {
                try {
                    await connection.OpenAsync().ConfigureAwait(false);
                }
                catch(Exception ex) {
                    throw new StorageUnavailableException(ex);
                }

                using(var transaction = connection.BeginTransaction(isolationLevel, transactionName)) {
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