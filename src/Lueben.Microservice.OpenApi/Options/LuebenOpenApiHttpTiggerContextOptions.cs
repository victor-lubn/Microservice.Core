using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Options
{
    public class LuebenOpenApiHttpTiggerContextOptions
    {
        public IOpenApiConfigurationOptions OpenApiConfigurationOptions { get; set; } = new DefaultOpenApiConfigurationOptions();

        public IOpenApiHttpTriggerAuthorization OpenApiHttpTriggerAuthorization { get; set; }

        public IOpenApiCustomUIOptions OpenApiCustomUIOptions { get; set; }

        public List<Action<VisitorCollection>> VisitorActions { get; set; } = new List<Action<VisitorCollection>>();

        public NamingStrategy NamingStrategy { get; set; } = new CamelCaseNamingStrategy();

        public List<OpenApiParameter> CommonOpenApiParameters { get; set; } = new List<OpenApiParameter>();
    }
}
