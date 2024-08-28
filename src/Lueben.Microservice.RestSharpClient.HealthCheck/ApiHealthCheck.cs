using System;
using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Lueben.Microservice.RestSharpClient.HealthCheck
{
    public class ApiHealthCheck : IHealthCheck
    {
        private readonly IRestSharpClientFactory _restSharpClientFactory;
        private readonly ILogger _logger;
        private readonly string _apiUrl;
        private readonly string _sectionName;

        public ApiHealthCheck(IRestSharpClientFactory restSharpClientFactory, ILogger<ApiHealthCheck> logger, string apiUrl, string sectionName)
        {
            _restSharpClientFactory = restSharpClientFactory ?? throw new ArgumentNullException(nameof(restSharpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _sectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var restClient = _restSharpClientFactory.Create(_sectionName);
                var request = new RestRequest(_apiUrl);
                await restClient.ExecuteRequestAsync(request);
                return HealthCheckResult.Healthy($"Service is available.");
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception, $"Heathcheck for {_apiUrl} failed with message: '{exception.Message}'.");
                return HealthCheckResult.Unhealthy("Service is not available.");
            }
        }
    }
}