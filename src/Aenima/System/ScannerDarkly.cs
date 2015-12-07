using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;

namespace Aenima.System
{
    internal static class ScannerDarkly
    {
        /// <summary>
        ///     returns all loadable types from all referenced assemblies.
        /// </summary>
        public static IEnumerable<Type> GetReferencedTypes()
        {
            return GetReferencedAssemblies().SelectMany(GetLoadableTypes);
        }

        /// <summary>
        ///     Returns all loadable types from all referenced assemblies, 
        /// that are assignable to a given base type, excluding that same base type.
        /// </summary>
        public static IEnumerable<Type> GetTypesAssignableTo(Type baseType)
        {
            if(baseType == null) throw new ArgumentNullException(nameof(baseType));

            return GetReferencedTypes()
                .Where(baseType.IsAssignableFrom)
                .Where(type => type != baseType);
        }

        /// <summary>
        ///     Returns all referenced assemblies.
        /// </summary>
        public static IEnumerable<Assembly> GetReferencedAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().Cast<Assembly>();
        } 

        /// <summary>
        ///     Gets all the loadable types from an assembly.
        /// </summary>
        /// <remarks>
        ///     Source: http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
        /// </remarks>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
          
            try {
                return assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException ex) {
                return ex.Types.Where(t => t != null);
            }
        }
    }
}