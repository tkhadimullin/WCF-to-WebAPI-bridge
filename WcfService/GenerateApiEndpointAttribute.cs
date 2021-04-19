using System;

namespace WcfService
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GenerateApiEndpointAttribute: Attribute
    {
    }
}