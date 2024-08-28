using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lueben.Microservice.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace Lueben.Microservice.HealthChecks.Models
{
    [ExcludeFromCodeCoverage]
    public class HealthCheckResult : IActionResult
    {
        public HealthCheckResponse HealthCheckResponse { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            SetStatusCode(context);
            await WriteBody(context);
        }

        private void SetStatusCode(ActionContext context)
        {
            var statusCode = HealthCheckResponse.Status == HealthStatus.Unhealthy.ToString() ?
                HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK;

            context.HttpContext.Response.StatusCode = (int)statusCode;
        }

        private async Task WriteBody(ActionContext context)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(HealthCheckResponse, FunctionJsonSerializerSettingsProvider.CreateSerializerSettings()));

            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await context.HttpContext.Response.Body.FlushAsync();
        }
    }
}
