using System.Data;
using Dapper;

namespace Aenima.Dapper.Extensions
{
    public static class DynamicParametersExtensions
    {
        public static void AddOutput(this DynamicParameters parameters, string name = "Result", DbType type = DbType.Int32)
        {
            parameters.Add(name, dbType: type, direction: ParameterDirection.Output);
        }

        public static T GetOutput<T>(this DynamicParameters parameters, string name = "Result")
        {
            return parameters.Get<T>(name);
        }

        public static void AddReturnValue(this DynamicParameters parameters)
        {
            parameters.Add("ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        }

        public static int GetReturnValue(this DynamicParameters parameters)
        {
            return parameters.Get<int>("ReturnValue");
        }
    }
}