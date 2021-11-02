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
                var impl = implType;
                if (service != null && impl != null)
                {
                    var lifeTimeAttr = (LifeTimeAttribute)Attribute.GetCustomAttribute(impl, typeof(LifeTimeAttribute));
                    var lifeTime = lifeTimeAttr != null ? lifeTimeAttr.lifetime : ServiceLifetime.Singleton;
                    services.TryAdd(new ServiceDescriptor(service, impl, lifeTime));
                }
            }
        }

        public static void RegisteFromExecutingAssembly(IServiceCollection services, Predicate<Type> typePredicate)
        {
            Registe(services,
                Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typePredicate(type)));
        }

        public static void RegisteService(IServiceCollection services)
        {
            RegisteFromExecutingAssembly(services,
                type => type.FullName.EndsWith("Service") && type.IsClass);

            RegisteFromExecutingAssembly(services,
                type => Attribute.IsDefined(type, typeof(ServiceAttribute)));
        }

        public static void RegisteRepository(IServiceCollection services)
        {
            RegisteFromExecutingAssembly(services,
                type => type.FullName.EndsWith("Repository") && type.IsClass);

            RegisteFromExecutingAssembly(services,
                type => Attribute.IsDefined(type, typeof(RepositoryAttribute)));
        }

        public static void RegisteComponent(IServiceCollection services)
        {
            RegisteFromExecutingAssembly(services,
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
