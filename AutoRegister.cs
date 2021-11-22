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
                        return BuildServiceDescriptor(service, implType, lifeTime);
                    });

                //將serviceDescriptors加入services
                foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
                    services.Add(serviceDescriptor);

                //建立實體對應ServiceDescriptor
                services.Add(BuildServiceDescriptor(implType, implType, lifeTime));

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

        private static ServiceDescriptor BuildServiceDescriptor(Type service, Type implType, ServiceLifetime lifetime)
        {
            //Factory
            Func<IServiceProvider, object> factory = (serviceProvider) => {

                #region 建構子注入

                //限單一建構子
                var con = implType.GetConstructors().Single();
                var conParams = con.GetParameters();
                var conParamObjects = new object[conParams.Length];
                
                for(int i = 0; i < conParamObjects.Length; i++)
                {
                    conParamObjects[i] = serviceProvider.GetRequiredService(conParams[i].ParameterType);
                }

                //建立實體
                object impl = Activator.CreateInstance(implType, conParamObjects);
                #endregion

                #region Field注入

                //取得含 AutoWired 屬性的 Field
                BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static;
                var fields = implType.GetFields(bindFlags).Where(field => Attribute.IsDefined(field, typeof(AutoWiredAttribute)));

                foreach(FieldInfo field in fields)
                {
                    //避免靜態Field被重新賦值
                    if (field.IsStatic)
                    {
                        if (field.GetValue(impl) != null)
                            continue;
                    }

                    object fieldImpl = serviceProvider.GetRequiredService(field.FieldType);
                    field.SetValue(impl, fieldImpl);
                }
                #endregion

                return impl;
            };

            return new ServiceDescriptor(service, factory, lifetime);
        }
    }
}
