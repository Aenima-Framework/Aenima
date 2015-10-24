using System;
using System.Linq;
using Aenima.System.Extensions;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Scanning;

namespace Aenima.Autofac
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// A ContainerBuilder extension method that scans all the application current domain assemblies,
        /// and registers all the implementations found of the given base type, in one simple method.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
           RegisterAll(this ContainerBuilder builder, Type serviceBaseType)
        {
            return builder
                .RegisterAssemblyTypes(AppDomain.CurrentDomain.GetReferencedAssemblies().ToArray())
                .Where(serviceBaseType.IsAssignableFrom)
                .As(serviceBaseType);
        }

        /// <summary>
        /// A ContainerBuilder extension method that scans all the application current domain assemblies,
        /// and registers all the implementations found of the given base type, in one simple method.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
           RegisterAll<TServiceBaseType>(this ContainerBuilder builder)
        {
            return builder.RegisterAll(typeof(TServiceBaseType));
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterGenerics(this ContainerBuilder builder, Type openGenericType, string key = null)
        {
            //var temp = AppDomain.CurrentDomain.GetReferencedAssemblies()
            //    .SelectMany(assembly => assembly.GetTypes())
            //    .Where(
            //        type => type
            //            .GetInterfaces()
            //            .Where(interfaceType => interfaceType.IsClosedTypeOf(openGenericType))
            //            .Count() > 0)
            //    .ToList();
            var assemblies = AppDomain.CurrentDomain.GetReferencedAssemblies().ToArray();

            return key == null
                ? builder
                    .RegisterAssemblyTypes(assemblies)
                    .As(type => type
                        .GetInterfaces()
                        .Where(interfaceType => interfaceType.IsClosedTypeOf(openGenericType)))
                : builder
                    .RegisterAssemblyTypes(assemblies)
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
            return builder.RegisterAssemblyModules(AppDomain.CurrentDomain.GetReferencedAssemblies().ToArray());
        }
    }
}
