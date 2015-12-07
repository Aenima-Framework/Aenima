using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Scanning;

namespace Aenima.Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterGenerics(this ContainerBuilder builder, Type openGenericType, string key = null)
        {
            var typeRegistrationBuilder = builder
                .RegisterAssemblyTypes(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray());

            return key == null
                ? typeRegistrationBuilder
                    .As(type => type
                        .GetInterfaces()
                        .Where(interfaceType => interfaceType.IsClosedTypeOf(openGenericType)))
                : typeRegistrationBuilder
                    .As(type => type
                        .GetInterfaces()
                        .Where(interfaceType => interfaceType.IsClosedTypeOf(openGenericType))
                        .Select(specificType => new KeyedService(key, specificType)));
        }

        public static void Decorate<TService>(
            this ContainerBuilder builder,
            string decoratedName,
            params Func<IComponentContext, TService, TService>[] factories)
        {
            var previousKey = decoratedName;

            for(var i = 0; i < factories.Length; i++)
            {
                var componentKey = $"{previousKey}-{Guid.NewGuid().ToString("N")}";

                var decoratorFactory = factories[i];

                var registration = builder
                    .RegisterDecorator<TService>((ctx, svc) =>
                        decoratorFactory(ctx, svc), fromKey: previousKey);

                // don't key the last decorator, since it will be the entry point instance
                if(i < factories.Length - 1)
                    registration.Keyed<TService>(componentKey);

                previousKey = componentKey;
            }
        }

        /// <summary>
        /// A ContainerBuilder extension method that scans all the application current domain assemblies,
        /// and registers all modules found, in one simple method.
        /// </summary>
        public static IModuleRegistrar RegisterAllModules(this ContainerBuilder builder)
        {
            return builder.RegisterAssemblyModules(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray());
        }
    }
}
