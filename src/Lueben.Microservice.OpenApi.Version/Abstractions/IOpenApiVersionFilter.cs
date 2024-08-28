using System.Collections.Generic;
using System.Reflection;

namespace Lueben.Microservice.OpenApi.Version.Abstractions
{
    public interface IOpenApiOperationFilter
    {
        IEnumerable<MethodInfo> Filter(IEnumerable<MethodInfo> methods);
    }
}