using System;
using System.Net;
using System.Threading.Tasks;
using Lueben.Microservice.ApplicationInsights;
using Lueben.Microservice.DurableFunction.Exceptions;
using Lueben.Microservice.DurableFunction.Extensions;
using Lueben.Microservice.RestSharpClient;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lueben.Microservice.DurableFunction
{
    public abstract class OrchestratorFunction<TData>
    {
        protected readonly TelemetryClient TelemetryClient;
        protected readonly ILogger<OrchestratorFunction<TData>> Logger;
        protected readonly ILoggerService LoggerService;

        protected OrchestratorFunction(TelemetryConfiguration telemetryConfiguration,
            ILogger<OrchestratorFunction<TData>> logger,
            ILoggerService loggerService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LoggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            TelemetryClient = new TelemetryClient(telemetryConfiguration ?? throw new ArgumentNullException(nameof(telemetryConfiguration)));
        }

        public abstract Task ProcessActivities(IDurableOrchestrationContext context, TData eventData);

        public async Task HandleErrors(IDurableOrchestrationContext context, bool useCorrelationTraceContext = true)
        {
            if (useCorrelationTraceContext)
            {
                TelemetryClient.TrackUsingCorrelationTraceContext();
            }

            try
            {
                await ProcessActivities(context, context.GetInput<TData>());
            }
            catch (FunctionFailedException wrappingException) when (wrappingException.InnerException is IncorrectEventDataException e)
            {
                Logger.LogError(e, "Stop event processing. Event is not processable: " + e.Message);
            }
            catch (FunctionFailedException wrappingException) when (wrappingException.InnerException is EventDataProcessFailureException e)
            {
                Logger.LogError(e, "Failed to process event: " + e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled error. Failed to process event.");
                throw;
            }
        }

        public virtual async Task HandleRestClientExceptions<TErrorResponse>(Func<TData, Task> apiCall, TData data)
        {
            try
            {
                await apiCall(data);
            }
            catch (Exception e)
            {
                await HandleRestClientExceptions<TData, TErrorResponse>(e);
            }
        }

        public virtual async Task HandleRestClientExceptions<TErrorResponse, TInput>(Func<TInput, Task> apiCall, TInput data)
        {
            try
            {
                await apiCall(data);
            }
            catch (Exception e)
            {
                await HandleRestClientExceptions<TData, TErrorResponse>(e);
            }
        }

        public virtual async Task<TOutput> HandleRestClientExceptions<TErrorResponse, TInput, TOutput>(Func<TInput, Task<TOutput>> apiCall, TInput data)
        {
            try
            {
                return await apiCall(data);
            }
            catch (Exception e)
            {
                await HandleRestClientExceptions<TInput, TErrorResponse>(e);
            }

            return default;
        }

        protected virtual string GetBadRequestErrorMessage(object response)
        {
            return JsonConvert.SerializeObject(response);
        }

        private Task HandleRestClientExceptions<TInput, TErrorResponse>(Exception e)
        {
            if (e is RestClientApiException restClientException)
            {
                switch (restClientException.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.UnprocessableEntity:
                        {
                            var errorMessage = GetBadRequestErrorMessage<TErrorResponse>(restClientException);
                            LoggerService.LogEvent(Events.EventFailed);
                            Logger.LogError(e, "Failed to process event.");
                            throw new IncorrectEventDataException(errorMessage);
                        }

                    default:
                        {
                            Logger.LogError(e, "Failed to process event.");
                            throw new EventDataProcessFailureException($"Failed to make request to {restClientException.Service}. {e.Message}.");
                        }
                }
            }

            Logger.LogError(e, $"Unexpected error occurred. {e.Message}");
            throw new EventDataProcessFailureException("Failed to send event data.");
        }

        private string GetBadRequestErrorMessage<TErrorResponse>(RestClientApiException exception)
        {
            var responseContent = exception?.ResponseContent;
            if (string.IsNullOrEmpty(responseContent))
            {
                return "No response content";
            }

            try
            {
                var response = JsonConvert.DeserializeObject<TErrorResponse>(responseContent);

                return GetBadRequestErrorMessage(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to deserialize {typeof(TErrorResponse).Name}: {responseContent}");
                return responseContent;
            }
        }
    }
}
