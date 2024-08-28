using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lueben.Microservice.RestSharpClient.Authentication
{
    public interface IServiceApiAuthorizer
    {
        Task<string> GetAccessTokenAsync(IReadOnlyCollection<string> scopes = null);
    }
}