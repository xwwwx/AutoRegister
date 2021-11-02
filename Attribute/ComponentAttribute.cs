using System;

namespace AutoRegister.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : ComponentAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RepositoryAttribute : ComponentAttribute
    {
    }
}
