using Aenima.DependencyResolution;
using Autofac;

namespace Aenima.Autofac
{
    public class AutofacDependencyResolverModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<AutofacDependencyResolver>()
                .As<IDependencyResolver>()
                .InstancePerLifetimeScope();
        }
    }
}