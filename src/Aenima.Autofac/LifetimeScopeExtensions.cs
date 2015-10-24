using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace Aenima.Autofac
{
    public static class LifetimeScopeExtensions
    {
        /// <summary>
        /// Enumerates all resolved service types in this collection.
        /// </summary>
        public static IEnumerable<TService> ResolveAll<TService>(this ILifetimeScope container)
            where TService : class
        {
            return container.Resolve<IEnumerable<TService>>() 
                ?? Enumerable.Empty<TService>();
        }

        /// <summary>
        /// Enumerates all resolved service types in this collection.
        /// </summary>
        public static IEnumerable<object> ResolveAll(this ILifetimeScope container, Type serviceType)
        {
            var collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instances      = container.Resolve(collectionType);

            return ((IEnumerable<object>)instances) 
                ?? Enumerable.Empty<object>();
        }
    }
}