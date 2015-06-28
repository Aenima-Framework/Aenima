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
using Dapper.Microsoft.Sql;

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

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new {
                StreamId        = streamId,
                StreamType      = "Not implemented yet.",
                ExpectedVersion = expectedVersion,
                Events          = new TableValuedParameter<NewStreamEvent>("Events", "StreamEvents", events)
            });

            parameters.AddOutput("Result");
            parameters.AddOutput("StreamVersion");
           
            // execute operation
            await WithTransaction(
                transactionName: "AppendStream",
                operation: async transaction => {
                    await transaction.ExecuteProcedure("AppendStream", parameters);         
                });

            // get output values
            var status        = parameters.GetOutput<int>("Result");
            var streamVersion = parameters.GetOutput<int>("StreamVersion");

            // validate operation result
            if(status == -1) {
                throw new StreamConcurrencyException(streamId, expectedVersion, streamVersion);
            }
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
                    result = await transaction.QueryProcedure<StreamEvent>("ReadStream", parameters);
                });

            var status = parameters.GetReturnValue();

            switch(status) {
                case -100: throw new StreamNotFoundException(streamId);
                case -200: throw new StreamDeletedException(streamId, fromVersion);
            }

            return StreamEventsPage.Create(
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
                    await connection.OpenAsync();
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