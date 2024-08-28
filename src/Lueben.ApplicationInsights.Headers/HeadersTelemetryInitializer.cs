using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Lueben.ApplicationInsights.Headers
{
    public class HeadersTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IList<string> _headers;

        public HeadersTelemetryInitializer(IHttpContextAccessor httpContextAccessor, IList<string> headers)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (_headers.Count == 0)
            {
                return;
            }

            if (!(telemetry is RequestTelemetry))
            {
                return;
            }

            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return;
            }

            var telemetryProps = ((ISupportProperties)telemetry).Properties;
            if (telemetryProps == null)
            {
                return;
            }

            AddHeaders(telemetryProps, context.Request.Headers, nameof(context.Request));
            AddHeaders(telemetryProps, context.Response.Headers, nameof(context.Response));
        }

        private void AddHeaders(IDictionary<string, string> telemetryProps, IHeaderDictionary headers, string headerLocation)
        {
            foreach (var headerName in _headers)
            {
                var headerValues = headers[headerName];
                if (headerValues.Any())
                {
                    telemetryProps.TryAdd($"{headerLocation}-{headerName}", string.Join(Environment.NewLine, headerValues.AsEnumerable()));
                }
            }
        }
    }
}