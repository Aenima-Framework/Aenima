
using Aenima.System.Extensions;

namespace Aenima.Dapper
{

    //aenima:dapper:connection

    //aenima:polly:retries:3
    //aenima:polly:retries-interval:1
    //aenima:polly:circuit-breaker-multiplier:1.5

    public class DapperEventStoreSettings
    {
        public DapperEventStoreSettings(string connectionString, string tableName = null, string tableSchema = null)
        {
            ConnectionString = connectionString;
            TableName        = tableName.IsNullOrWhiteSpace() ? "EventStream" : tableName;
            TableSchema      = tableSchema.IsNullOrWhiteSpace() ? "dbo" : tableSchema ;
            FullTableName    = "{0}.{1}".FormatWith(TableSchema, TableName);
        }
       
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets or sets the table schema.
        /// <para>Default: 'dbo'</para>
        /// </summary>
        /// <value>The  table schema.</value>
        public string TableSchema { get; private set; }

        /// <summary>
        /// Gets or sets the table name.
        /// <para>Default: 'EventStream'</para>
        /// </summary>
        /// <value>The table name.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets or sets the full table name, including the schema.
        /// <para>Default: 'dbo.EventStream'</para>
        /// </summary>
        /// <value>The full table name, including the schema.</value>
        public string FullTableName { get; private set; }
    }
}