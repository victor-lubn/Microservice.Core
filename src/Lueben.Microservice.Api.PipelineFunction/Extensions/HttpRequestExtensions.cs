using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Lueben.Microservice.Api.PipelineFunction.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetHeaderValueOrDefault(this HttpRequest request, string headerName)
        {
            return request.Headers.TryGetValue(headerName, out var values) ? values.FirstOrDefault() : null;
        }
    }
}
