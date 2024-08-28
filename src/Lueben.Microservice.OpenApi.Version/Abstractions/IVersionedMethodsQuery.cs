using System.Collections.Generic;
using System.Reflection;

namespace Lueben.Microservice.OpenApi.Version.Abstractions
{
    public interface IVersionedMethodsQuery
    {
        List<MethodInfo> Get(Assembly assembly);
    }
}
