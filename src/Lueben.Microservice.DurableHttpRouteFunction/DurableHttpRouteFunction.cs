using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.DurableFunction;
using Lueben.Microservice.DurableFunction.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Lueben.Microservice.DurableHttpRouteFunction
{
    public class DurableHttpRouteFunction
    {
        private readonly DurableTaskOptions _durableTaskOptions;
        private readonly ILogger<DurableHttpRouteFunction> _logger;
        private readonly IOptionsSnapshot<WorkflowOptions> _options;
        private readonly IConfiguration _configuration;
        private readonly TelemetryClient _telemetryClient;

        public DurableHttpRouteFunction(TelemetryConfiguration telemetryConfiguration,
            ILogger<DurableHttpRouteFunction> logger,
            IOptionsSnapshot<WorkflowOptions> options,
            IOptions<DurableTaskOptions> durableTaskOptions,
            IConfiguration configuration)
        {
            _durableTaskOptions = durableTaskOptions?.Value ?? throw new ArgumentNullException(nameof(durableTaskOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _telemetryClient = new TelemetryClient(telemetryConfiguration ?? throw new ArgumentNullException(nameof(telemetryConfiguration)));
        }

        [FunctionName(nameof(Orchestrator))]
        public async Task Orchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (_durableTaskOptions.Tracing.DistributedTracingEnabled &&
                _durableTaskOptions.Tracing.Version != DurableDistributedTracingVersion.V2)
            {
                _telemetryClient.TrackUsingCorrelationTraceContext();
            }

            var eventData = context.GetInput<RetryData<HttpRouteInput>>();
            var request = BuildDurableHttpRequest(eventData);

            DurableHttpResponse response = null;
            try
            {
                response = await context.CallHttpAsync(request);
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Failed to send event data.");
            }

            if (response == null || RetryRequired(response, eventData.Retry))
            {
                var retryTime = context.CurrentUtcDateTime.Add(_options.Value.ActivityRetryIntervalTime);
                await context.CreateTimer(retryTime, CancellationToken.None);

                eventData.Retry++;
                context.ContinueAsNew(eventData);

                _logger.LogInformation($"Started retry orchestration. Retry count: {eventData.Retry}.");
            }
        }

        private bool RetryRequired(DurableHttpResponse result, int retryAttempt)
        {
            switch (result.StatusCode)
            {
                case HttpStatusCode.Accepted:
                case HttpStatusCode.OK:
                {
                    _logger.LogInformation("Routed event to service.");
                    return false;
                }

                case HttpStatusCode.BadRequest:
                {
                    _logger.LogError("EventData is invalid. " + result.Content);
                    return false;
                }

                default:
                {
                    _logger.LogError($"Failed to process event. Error Code {result.StatusCode}. {result.Content}. Scheduling a retry.");
                    if (_options.Value.MaxEventRetryCount == 0 || retryAttempt < _options.Value.MaxEventRetryCount)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        private DurableHttpRequest BuildDurableHttpRequest(RetryData<HttpRouteInput> eventData)
        {
            IDictionary<string, StringValues> headers = null;
            if (!string.IsNullOrEmpty(eventData.Input.HandlerOptions.FunctionKey))
            {
                var functionKey = GetFunctionKey(eventData.Input.HandlerOptions);
                headers = new ConcurrentDictionary<string, StringValues> { ["x-functions-key"] = functionKey };
            }

            var request = new DurableHttpRequest(HttpMethod.Post,
                new Uri(eventData.Input.HandlerOptions.ServiceUrl),
                headers,
                eventData.Input.Payload,
                null,
                false,
                _options.Value.ActivityRetryIntervalTime);

            return request;
        }

        private string GetFunctionKey(HttpEventHandlerOptions httpEventHandlerOptions)
        {
            var functionKey = IsConfigurationOption(httpEventHandlerOptions.FunctionKey)
                ? _configuration.GetSection(httpEventHandlerOptions.FunctionKey).Get<string>()
                : httpEventHandlerOptions.FunctionKey;

            return functionKey;
        }

        private bool IsConfigurationOption(string key)
        {
            return key.StartsWith(nameof(HttpEventHandlerOptions));
        }
    }
}