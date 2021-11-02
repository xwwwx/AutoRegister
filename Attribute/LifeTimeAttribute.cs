using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoRegister.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LifeTimeAttribute : Attribute
    {
        public ServiceLifetime lifetime;

        public LifeTimeAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) => this.lifetime = lifetime;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : LifeTimeAttribute
    {
        public SingletonAttribute() : base(ServiceLifetime.Singleton) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ScopedAttribute : LifeTimeAttribute
    {
        public ScopedAttribute() : base(ServiceLifetime.Scoped) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TransientAttribute : LifeTimeAttribute
    {
        public TransientAttribute() : base(ServiceLifetime.Transient) { }
    }
}
