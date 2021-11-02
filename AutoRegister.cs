using AutoRegister.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoRegister
{
    public static class AutoRegister
    {
        public static void Registe(IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (Type implType in types)
            {
                var service = implType.GetInterfaces().SingleOrDefault(type => type.Name.Contains(implType.Name));
                if (implType != null)
                {
                    if (service == null)
                        service = implType;
                    var lifeTimeAttr = (LifeTimeAttribute)Attribute.GetCustomAttribute(implType, typeof(LifeTimeAttribute));
                    var lifeTime = lifeTimeAttr != null ? lifeTimeAttr.lifetime : ServiceLifetime.Singleton;
                    services.TryAdd(new ServiceDescriptor(service, implType, lifeTime));
                }
            }
        }

        public static void RegisteFromEntryAssembly(IServiceCollection services, Predicate<Type> typePredicate)
        {
            Registe(services,
                Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(type => typePredicate(type)));
        }

        public static void RegisteService(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => type.FullName.EndsWith("Service") && type.IsClass);

            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(ServiceAttribute)));
        }

        public static void RegisteRepository(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => type.FullName.EndsWith("Repository") && type.IsClass);

            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(RepositoryAttribute)));
        }

        public static void RegisteComponent(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(ComponentAttribute)));
        }

        public static void AutoRegiste(this IServiceCollection services)
        {
            RegisteComponent(services);

            RegisteService(services);

            RegisteRepository(services);
        }
    }
}
