using System;

namespace AutoRegister.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
    }

    public class ServiceAttribute : ComponentAttribute
    {
    }

    public class RepositoryAttribute : ComponentAttribute
    {
    }
}
