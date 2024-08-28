using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace Lueben.Microservice.RestSharpClient
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class RestClientApiException : Exception
    {
        public string Service { get; }

        public string ResponseContent { get; }

        public object ResponseData { get; set; }

        public HttpStatusCode? StatusCode { get; }

        public RestClientApiException()
        {
        }

        public RestClientApiException(string service) : this(service, null)
        {
        }

        public RestClientApiException(string service, Exception innerException) : this(service, null, null, innerException)
        {
        }

        public RestClientApiException(string service, string responseContent, HttpStatusCode? statusCode, Exception innerException) : base(GetErrorMessage(service), innerException)
        {
            Service = service;
            ResponseContent = responseContent;
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (StatusCode.HasValue)
            {
                sb.Append($"{nameof(StatusCode)}: {StatusCode};");
            }

            if (ResponseContent != null)
            {
                sb.Append($"{nameof(ResponseContent)}: {ResponseContent};");
            }

            return sb + base.ToString();
        }

        protected RestClientApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetErrorMessage(string service) => $"Error during request to: {service}.";
    }
}