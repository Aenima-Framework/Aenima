using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aenima.System.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        ///     Gets the loadable types for the given assembly.
        /// </summary>
        /// <remarks>
        ///     Source: http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
        /// </remarks>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try {
                return assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException ex) {
                return ex.Types.Where(t => t != null);
            }
        }
    }
}