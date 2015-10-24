using System;
using System.Collections.Generic;
using Aenima.DependencyResolution;
using Autofac;

namespace Aenima.Autofac
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly ILifetimeScope _container;

        public AutofacDependencyResolver(ILifetimeScope container)
        {
            _container = container;
        }

        public TService Resolve<TService>() where TService : class
        {
            return _container.Resolve<TService>();
        }

        public IEnumerable<TService> ResolveAll<TService>() where TService : class
        {
            return _container.ResolveAll<TService>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _container.ResolveAll(serviceType);
        }
    }
}