using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Aenima.Dapper.Extensions;
using Aenima.EventStore;
using Aenima.Exceptions;
using Aenima.System;
using Dapper;
using Dapper.Microsoft.Sql;
using Microsoft.SqlServer.Server;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PossibleMultipleEnumeration

namespace Aenima.Dapper
{
    public class TableValuedParameter<T> : SqlMapper.IDynamicParameters
    {
        private static readonly Dictionary<SqlDbType, Action<SqlDataRecord, object>> actionMappings = new Dictionary
            <SqlDbType, Action<SqlDataRecord, object>>()
        {
            {
                SqlDbType.Bit,
                (Action<SqlDataRecord, object>)((record, o) => record.SetBoolean(0, Convert.ToBoolean(o)))
            },
            {
                SqlDbType.TinyInt,
                (Action<SqlDataRecord, object>)((record, o) => record.SetByte(0, Convert.ToByte(o)))
            },
            {
                SqlDbType.SmallInt,
                (Action<SqlDataRecord, object>)((record, o) => record.SetInt16(0, Convert.ToInt16(o)))
            },
            {
                SqlDbType.Int,
                (Action<SqlDataRecord, object>)((record, o) => record.SetInt32(0, Convert.ToInt32(o)))
            },
            {
                SqlDbType.BigInt,
                (Action<SqlDataRecord, object>)((record, o) => record.SetInt64(0, Convert.ToInt64(o)))
            },
            {
                SqlDbType.NChar,
                (Action<SqlDataRecord, object>)((record, o) => record.SetValue(0, o))
            },
            {
                SqlDbType.NVarChar,
                (Action<SqlDataRecord, object>)((record, o) => record.SetValue(0, o))
            }
        };

        private static readonly Dictionary<Type, SqlDbType> typeMappings = new Dictionary<Type, SqlDbType>()
        {
            {
                typeof(bool),
                SqlDbType.Bit
            },
            {
                typeof(byte),
                SqlDbType.TinyInt
            },
            {
                typeof(short),
                SqlDbType.SmallInt
            },
            {
                typeof(int),
                SqlDbType.Int
            },
            {
                typeof(long),
                SqlDbType.BigInt
            },
            {
                typeof(char),
                SqlDbType.NChar
            },
            {
                typeof(string),
                SqlDbType.NVarChar
            }
        };

        private readonly Action<SqlDataRecord, object> actionMapping;
        private readonly string name;
        private readonly SqlMetaData[] tvpDefinition;
        private readonly string typeName;
        private readonly IEnumerable<T> values;

        public TableValuedParameter(string name, string typeName, IEnumerable<T> values)
        {
            this.name = name;
            this.typeName = typeName;
            this.values = values;
            this.tvpDefinition = new SqlMetaData[1]
            {
                typeof(T) == typeof(char)
                    ? new SqlMetaData("n", typeMappings[typeof(T)], 1L)
                    : (typeof(T) == typeof(string)
                        ? new SqlMetaData(
                            "n",
                            typeMappings[typeof(T)],
                            2048L)
                        : new SqlMetaData("n", typeMappings[typeof(T)]))
            };
            this.actionMapping = actionMappings[typeMappings[typeof(T)]];
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            var sqlCommand = (SqlCommand)command;
            sqlCommand.CommandType = CommandType.StoredProcedure;
            var list =
                Enumerable.ToList<SqlDataRecord>(
                    Enumerable.Select<T, SqlDataRecord>(
                        this.values,
                        (Func<T, SqlDataRecord>)(s => {
                            var sqlDataRecord = new SqlDataRecord(this.tvpDefinition);
                            this.actionMapping(sqlDataRecord, (object)s);
                            return sqlDataRecord;
                        })));
            var sqlParameter = sqlCommand.Parameters.Add(this.name, SqlDbType.Structured);
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.TypeName = this.typeName;
            sqlParameter.Value = (object)list;
        }
    }

    public static class EnumerableExtensions
    {
        /// <summary>
        /// This extension converts an enumerable set to a Dapper TVP
        /// </summary>
        /// <typeparam name="T">type of enumerable</typeparam>
        /// <param name="enumerable">list of values</param>
        /// <param name="typeName">database type name</param>
        /// <param name="orderedColumnNames">if more than one column in a TVP, columns order must match order of columns in TVP</param>
        /// <returns>a custom query parameter</returns>
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> enumerable,
            string typeName, IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();
            if(typeof(T).IsValueType)
            {
                dataTable.Columns.Add("NONAME", typeof(T));
                foreach(T obj in enumerable)
                {
                    dataTable.Rows.Add(obj);
                }
            }
            else
            {
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] readableProperties = properties.Where(w => w.CanRead).ToArray();
                if(readableProperties.Length > 1 && orderedColumnNames == null)
                    throw new ArgumentException("Ordered list of column names must be provided when TVP contains more than one column");
                var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name)).ToArray();
                foreach(string name in columnNames)
                {
                    dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
                }

                foreach(T obj in enumerable)
                {
                    dataTable.Rows.Add(
                        columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
                            .ToArray());
                }
            }
            return dataTable.AsTableValuedParameter(typeName);
        }
    }

    public class ObjectShredder<T>
    {
        private FieldInfo[] _fi;
        private PropertyInfo[] _pi;
        private Dictionary<string, int> _ordinalMap;
        private Type _type;

        // ObjectShredder constructor. 
        public ObjectShredder()
        {
            _type = typeof(T);
            _fi = _type.GetFields();
            _pi = _type.GetProperties();
            _ordinalMap = new Dictionary<string, int>();
        }

        /// <summary> 
        /// Loads a DataTable from a sequence of objects. 
        /// </summary> 
        /// <param name="source">The sequence of objects to load into the DataTable.</param>
        /// <param name="table">The input table. The schema of the table must match that 
        /// the type T.  If the table is null, a new table is created with a schema  
        /// created from the public properties and fields of the type T.</param> 
        /// <param name="options">Specifies how values from the source sequence will be applied to 
        /// existing rows in the table.</param> 
        /// <returns>A DataTable created from the source sequence.</returns> 
        public DataTable Shred(IEnumerable<T> source, DataTable table = null, LoadOption? options = null)
        {
            // Load the table from the scalar sequence if T is a primitive type. 
            if(typeof(T).IsPrimitive)
            {
                return ShredPrimitive(source, table, options);
            }

            // Create a new table if the input table is null. 
            if(table == null)
            {
                table = new DataTable(typeof(T).Name);
            }

            // Initialize the ordinal map and extend the table schema based on type T.
            table = ExtendTable(table, typeof(T));

            // Enumerate the source sequence and load the object values into rows.
            table.BeginLoadData();
            using(IEnumerator<T> e = source.GetEnumerator())
            {
                while(e.MoveNext())
                {
                    if(options != null)
                    {
                        table.LoadDataRow(ShredObject(table, e.Current), (LoadOption)options);
                    }
                    else
                    {
                        table.LoadDataRow(ShredObject(table, e.Current), true);
                    }
                }
            }
            table.EndLoadData();

            // Return the table. 
            return table;
        }

        public DataTable ShredPrimitive(IEnumerable<T> source, DataTable table, LoadOption? options)
        {
            // Create a new table if the input table is null. 
            if(table == null)
            {
                table = new DataTable(typeof(T).Name);
            }

            if(!table.Columns.Contains("Value"))
            {
                table.Columns.Add("Value", typeof(T));
            }

            // Enumerate the source sequence and load the scalar values into rows.
            table.BeginLoadData();
            using(IEnumerator<T> e = source.GetEnumerator())
            {
                Object[] values = new object[table.Columns.Count];
                while(e.MoveNext())
                {
                    values[table.Columns["Value"].Ordinal] = e.Current;

                    if(options != null)
                    {
                        table.LoadDataRow(values, (LoadOption)options);
                    }
                    else
                    {
                        table.LoadDataRow(values, true);
                    }
                }
            }
            table.EndLoadData();

            // Return the table. 
            return table;
        }

        public object[] ShredObject(DataTable table, T instance)
        {

            FieldInfo[] fi = _fi;
            PropertyInfo[] pi = _pi;

            if(instance.GetType() != typeof(T))
            {
                // If the instance is derived from T, extend the table schema 
                // and get the properties and fields.
                ExtendTable(table, instance.GetType());
                fi = instance.GetType().GetFields();
                pi = instance.GetType().GetProperties();
            }

            // Add the property and field values of the instance to an array.
            Object[] values = new object[table.Columns.Count];
            foreach(FieldInfo f in fi)
            {
                values[_ordinalMap[f.Name]] = f.GetValue(instance);
            }

            foreach(PropertyInfo p in pi)
            {
                values[_ordinalMap[p.Name]] = p.GetValue(instance, null);
            }

            // Return the property and field values of the instance. 
            return values;
        }

        public DataTable ExtendTable(DataTable table, Type type)
        {
            // Extend the table schema if the input table was null or if the value  
            // in the sequence is derived from type T.             
            foreach(FieldInfo f in type.GetFields())
            {
                if(!_ordinalMap.ContainsKey(f.Name))
                {
                    // Add the field as a column in the table if it doesn't exist 
                    // already.
                    DataColumn dc = table.Columns.Contains(f.Name) ? table.Columns[f.Name]
                        : table.Columns.Add(f.Name, f.FieldType);

                    // Add the field to the ordinal map.
                    _ordinalMap.Add(f.Name, dc.Ordinal);
                }
            }
            foreach(PropertyInfo p in type.GetProperties())
            {
                if(!_ordinalMap.ContainsKey(p.Name))
                {
                    // Add the property as a column in the table if it doesn't exist 
                    // already.
                    DataColumn dc = table.Columns.Contains(p.Name) ? table.Columns[p.Name]
                        : table.Columns.Add(p.Name, p.PropertyType);

                    // Add the property to the ordinal map.
                    _ordinalMap.Add(p.Name, dc.Ordinal);
                }
            }

            // Return the table. 
            return table;
        }
    }


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


            var temp = new ObjectShredder<NewStreamEvent>()
                .Shred(events)
                .AsTableValuedParameter();

            // create parameters
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(new {
                StreamId              = streamId,
                StreamType            = "Not implemented yet.",
                ExpectedStreamVersion = expectedVersion,
                StreamEvents          = temp
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