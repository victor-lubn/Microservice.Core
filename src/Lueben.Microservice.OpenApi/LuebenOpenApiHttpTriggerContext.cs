using Lueben.Microservice.OpenApi.Options;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi
{
    public class LuebenOpenApiHttpTriggerContext : OpenApiHttpTriggerContext
    {
        private readonly NamingStrategy _namingStrategy;
        private readonly VisitorCollection _visitorCollection;

        public LuebenOpenApiHttpTriggerContext(LuebenOpenApiHttpTiggerContextOptions options)
            : base(options.OpenApiConfigurationOptions, options.OpenApiHttpTriggerAuthorization, options.OpenApiCustomUIOptions)
        {
            _namingStrategy = options.NamingStrategy;
            _visitorCollection = InitializeVisitors(options);
        }

        public override NamingStrategy NamingStrategy => _namingStrategy ?? base.NamingStrategy;

        public override VisitorCollection GetVisitorCollection() => _visitorCollection;

        private VisitorCollection InitializeVisitors(LuebenOpenApiHttpTiggerContextOptions options)
        {
            var visitors = base.GetVisitorCollection();

            foreach (var visitorAction in options.VisitorActions)
            {
                visitorAction(visitors);
            }

            return visitors;
        }
    }
}
