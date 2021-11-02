using System;

namespace AutoRegister.Attributes
{
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
