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
        private static readonly ISet<Type> registedType = new HashSet<Type>();

        public static void Registe(IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (Type implType in types.Where(type => type != null && !registedType.Contains(type)))
            {

                var lifeTimeAttr = (LifeTimeAttribute)Attribute.GetCustomAttribute(implType, typeof(LifeTimeAttribute));
                var lifeTime = lifeTimeAttr != null ? lifeTimeAttr.lifetime : ServiceLifetime.Singleton;

                var serviceDescriptors = 
                    implType.GetInterfaces()?
                    .Select(service => {
                        return new ServiceDescriptor(service, implType, lifeTime);
                    })
                    .ToList();

                serviceDescriptors.Add(new ServiceDescriptor(implType, implType, lifeTime));
                
                foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
                    services.Add(serviceDescriptor);

                registedType.Add(implType);
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
                type => type.FullName.EndsWith("Service") && type.IsClass && !type.IsAbstract);

            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(ServiceAttribute)));
        }

        public static void RegisteRepository(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => type.FullName.EndsWith("Repository") && type.IsClass && !type.IsAbstract);

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
