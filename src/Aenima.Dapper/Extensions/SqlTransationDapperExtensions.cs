using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace Aenima.Dapper.Extensions
{
    public static class SqlTransationDapperExtensions
    {
        public static Task<IEnumerable<T>> QueryProcedure<T>(this SqlTransaction transaction, string procedure, DynamicParameters parameters)
        {
            return transaction.Connection
                .QueryAsync<T>(
                    procedure,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction);
        }

        public static Task<int> ExecuteProcedure(this SqlTransaction transaction, string procedure, DynamicParameters parameters)
        {
            return transaction.Connection
                .ExecuteAsync(
                    procedure,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction);
        }

    }
}