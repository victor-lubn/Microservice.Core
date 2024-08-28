using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Lueben.Microservice.RestSharpClient.Authentication
{
    public class AadConfidentialClient : IServiceApiAuthorizer
    {
        private readonly Lazy<IConfidentialClientApplication> _confidentialClient;

        public AadConfidentialClient(IOptions<ConfidentialClientApplicationOptions> options)
        {
            _confidentialClient = new Lazy<IConfidentialClientApplication>(
                () => ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(options.Value).Build());
        }

        public async Task<string> GetAccessTokenAsync(IReadOnlyCollection<string> scopes = null)
        {
            var result = await _confidentialClient.Value.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }
    }
}