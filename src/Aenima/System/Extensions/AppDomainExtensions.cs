using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Aenima.System.Extensions
{
    public static class AppDomainExtensions
    {
        private static readonly ConcurrentBag<string> PreLoadedDomains = new ConcurrentBag<string>();

        /// <summary>
        /// Gets the types from the assemblies that have been loaded into the execution context of this application domain.
        /// </summary>
        public static IEnumerable<Type> GetTypes(this AppDomain domain)
        {
            var types = domain
                .GetAssemblies()
                .SelectMany(a => a.GetLoadableTypes());

            return types;
        }

        /// <summary>
        /// Gets the types from the assemblies that have been loaded into the execution context of this application domain, 
        /// and that are assignable to a given type, excluding that same type.
        /// </summary>
        public static IEnumerable<Type> GetTypesAssignableTo(this AppDomain domain, Type baseType)
        {
            var types = domain
                .GetTypes()
                .Where(baseType.IsAssignableFrom)
                .Where(type => type != baseType);

            return types;
        }

        /// <summary>
        /// Gets the types from the assemblies that are referenced to this domain.
        /// </summary>
        public static IEnumerable<Type> GetReferencedTypes(this AppDomain domain)
        {
            var types = domain
                .GetReferencedAssemblies()
                .SelectMany(a => a.GetLoadableTypes());

            return types;
        }

        /// <summary>
        /// Gets the referenced assemblies in this collection, by scanning the application domain's base directory.
        /// </summary>
        /// <remarks>
        /// Because of the JIT(Just in time) mechanism of .NET,
        /// not all the assemblies are yet loaded and thus reflection will not return all referenced assemblies.
        /// </remarks>
        public static IEnumerable<Assembly> GetReferencedAssemblies(this AppDomain domain, bool loadSymbols = true)
        {
            domain.PreloadReferencedAssemblies();

            return domain
                .GetAssemblies()
                .ToList()
                .Distinct(EqualityHelper<Assembly>.CreateComparer(assembly => assembly.FullName));
        }

        /// <summary>
        /// An AppDomain extension method that preloads referenced assemblies, 
        /// by scanning the application domain's base directory.
        /// </summary>
        /// <remarks>
        ///     Assembly.LoadFile(filename); --> this can not be done, because this will lock the file and we will have problems when rebuilding the application
        ///     http://stackoverflow.com/questions/1031431/system-reflection-assembly-loadfile-locks-file
        /// </remarks>
        public static void PreloadReferencedAssemblies(this AppDomain domain, bool loadSymbols = true)
        {
            if(PreLoadedDomains.Contains(domain.FriendlyName)) {
                return;
            }

            PreLoadedDomains.Add(domain.FriendlyName);

            var files = Directory.EnumerateFiles(domain.BaseDirectory, "*.dll", SearchOption.AllDirectories);

            if(loadSymbols)
            {
                var symbols = Directory
                    .EnumerateFiles(domain.BaseDirectory, "*.pdb", SearchOption.AllDirectories)
                    .ToDictionary(file => file.Substring(0, file.Length - 3) + "dll");

                files.WithEach(file => 
                {
                    if(symbols.ContainsKey(file))
                        domain.Load(File.ReadAllBytes(file), File.ReadAllBytes(symbols[file]));
                    else
                        domain.Load(File.ReadAllBytes(file));
                });
            }
            else
            {
                files.WithEach(file => domain.Load(File.ReadAllBytes(file)));
            }
        }
    }
}