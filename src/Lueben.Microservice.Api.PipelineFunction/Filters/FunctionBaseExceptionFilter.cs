using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#pragma warning disable 618

namespace Lueben.Microservice.Api.PipelineFunction.Filters
{
    public abstract class FunctionBaseExceptionFilter : IFunctionExceptionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FunctionBaseExceptionFilter> _logger;

        protected FunctionBaseExceptionFilter(IHttpContextAccessor httpContextAccessor,
            ILogger<FunctionBaseExceptionFilter> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            JsonConvert.DefaultSettings = FunctionJsonSerializerSettingsProvider.CreateSerializerSettings;
        }

        public virtual async Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            if (!PipelineFunction.IsPipelineEnabled(exceptionContext))
            {
                return;
            }

            _logger.LogDebug("Found an exception trying to execute the function.");

            var exception = exceptionContext.Exception.InnerException ?? exceptionContext.Exception;

            var errorResult = GetErrorResult(exception);
            if (errorResult == null)
            {
                exceptionContext.ExceptionDispatchInfo.Throw();
            }

            var response = _httpContextAccessor.HttpContext.Response;
            if (!response.HasStarted)
            {
                var result = JsonConvert.SerializeObject(errorResult);

                response.ContentType = MediaTypeNames.Application.Json;
                response.StatusCode = (int)errorResult.StatusCode;
                response.ContentLength = result.Length;

                if (Activity.Current != null)
                {
                    errorResult.DebugId = Activity.Current.RootId;
                }

                await response.WriteAsync(result, cancellationToken);
            }
        }

        public abstract ErrorResult GetErrorResult(Exception exception);
    }
}