using AutoRegister.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoRegister
{
    public static class AutoRegister
    {
        //紀錄已註冊的TYPE
        private static readonly ISet<Type> registedType = new HashSet<Type>();

        /// <summary>
        /// 將參數的Type註冊至IServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="types"></param>
        public static void Registe(IServiceCollection services, IEnumerable<Type> types)
        {
            //過濾已註冊及 Type is null
            foreach (Type implType in types.Where(type => type != null && !registedType.Contains(type)))
            {
                //取得生命週期
                var lifeTimeAttr = (LifeTimeAttribute)Attribute.GetCustomAttribute(implType, typeof(LifeTimeAttribute));
                var lifeTime = lifeTimeAttr.lifetime;

                //建立介面及實體的對應ServiceDescriptor
                var serviceDescriptors =
                    implType.GetInterfaces()?
                    .Select(service =>
                    {
                        return new ServiceDescriptor(service, implType, lifeTime);
                    });

                //將serviceDescriptors加入services
                foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
                    services.Add(serviceDescriptor);

                //建立實體對應ServiceDescriptor
                services.Add(new ServiceDescriptor(implType, implType, lifeTime));

                //紀錄已註冊Type
                registedType.Add(implType);
            }
        }

        /// <summary>
        /// 註冊 Assembly 內，並 Predicate<Type> 回傳為 True 的 Type
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="typePredicate"></param>
        public static void RegisteFromAssembly(IServiceCollection services, Assembly assembly, Predicate<Type> typePredicate)
        {
            Registe(services,
                assembly
                .GetTypes()
                .Where(type => typePredicate(type)));
        }

        /// <summary>
        /// 註冊 Assembly.GetEntryAssembly 內，並 Predicate<Type> 回傳為 True 的 Type
        /// </summary>
        /// <param name="services"></param>
        /// <param name="typePredicate"></param>
        public static void RegisteFromEntryAssembly(IServiceCollection services, Predicate<Type> typePredicate)
        {
            RegisteFromAssembly(services,
                Assembly.GetEntryAssembly(),
                typePredicate);
        }

        /// <summary>
        /// 註冊 EntryAssembly 內的 Service
        /// </summary>
        /// <param name="services"></param>
        public static void RegisteService(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => type.FullName.EndsWith("Service") && type.IsClass && !type.IsAbstract);

            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(ServiceAttribute)));
        }

        /// <summary>
        /// 註冊 EntryAssembly 內的 Repository
        /// </summary>
        /// <param name="services"></param>
        public static void RegisteRepository(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => type.FullName.EndsWith("Repository") && type.IsClass && !type.IsAbstract);

            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(RepositoryAttribute)));
        }

        /// <summary>
        /// 註冊 EntryAssembly 內的 Component
        /// </summary>
        /// <param name="services"></param>
        public static void RegisteComponent(IServiceCollection services)
        {
            RegisteFromEntryAssembly(services,
                type => Attribute.IsDefined(type, typeof(ComponentAttribute)));
        }

        /// <summary>
        /// 自動註冊 Extension
        /// </summary>
        /// <param name="services"></param>
        public static void AutoRegiste(this IServiceCollection services)
        {
            RegisteComponent(services);

            RegisteService(services);

            RegisteRepository(services);
        }
    }
}
