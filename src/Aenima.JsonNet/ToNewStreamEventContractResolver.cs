using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aenima.JsonNet
{
    internal class ToNewStreamEventContractResolver : DefaultContractResolver 
    {
        public static IContractResolver Instance => new ToNewStreamEventContractResolver();

        private readonly string[] metadataProperties = typeof(IDomainEvent).GetProperties().Select( p => p.Name).ToArray();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => base.CreateProperty(p, memberSerialization));

            var fields = type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(f => base.CreateProperty(f, memberSerialization));

            var all = props.Union(fields)
                .Where(jp => !this.metadataProperties.Contains(jp.PropertyName))
                .ToList();

            return all;
        }

        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {    
            var prop = base.CreateProperty(member, memberSerialization);

            if(!prop.Writable)
            {
                var property = member as PropertyInfo;
                if(property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;    
        }
    }
}