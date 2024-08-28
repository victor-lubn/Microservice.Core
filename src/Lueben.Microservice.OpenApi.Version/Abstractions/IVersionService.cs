using System.Collections.Generic;
using System.Reflection;

namespace Lueben.Microservice.OpenApi.Version.Abstractions
{
    public interface IVersionService
    {
        string GetVersion(IList<MethodInfo> methods);
    }
}
