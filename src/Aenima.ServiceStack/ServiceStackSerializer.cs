using System;
using ServiceStack.Text;

namespace Aenima.ServiceStack
{
    public class ServiceStackSerializer : ISerializer
    {
        public ServiceStackSerializer()
        {
            JsConfig.DateHandler                   = DateHandler.ISO8601;
            JsConfig.ExcludeTypeInfo               = true;
            JsConfig.IncludeTypeInfo               = false;
            JsConfig.IncludeNullValues             = false;
            JsConfig.TryToParsePrimitiveTypeValues = true;
        }

        public string Serialize(object obj)
        {        
            return JsonSerializer.SerializeToString(obj, obj.GetType());
        }

        public object Deserialize(string text, Type type)
        {
            return JsonSerializer.DeserializeFromString(text, type);
        }
    }
}
