using System;
using Lueben.Microservice.OpenApi.Options;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Configurations.AppSettings.Resolvers;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Extensions
{
    public static class LuebenOpenApiHttpTiggerContextOptionsExtensions
    {
        public static LuebenOpenApiHttpTiggerContextOptions UseOpenApiConfigurationFile(
            this LuebenOpenApiHttpTiggerContextOptions options,
            string fileName,
            IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                var argumentName = nameof(fileName);
                throw new ArgumentException($"'{argumentName}' cannot be null or empty.", argumentName);
            }

            var basePath = ConfigurationResolver.GetBasePath(configuration);

            var openApiConfig = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(fileName, true)
                .Build();

            openApiConfig.Bind(options.OpenApiConfigurationOptions);

            return options;
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddVisitor<TVisitor>(this LuebenOpenApiHttpTiggerContextOptions options)
            where TVisitor : IVisitor
        {
            return options.AddVisitor(vc => (IVisitor)Activator.CreateInstance(typeof(TVisitor), vc));
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddVisitor<TVisitor>(this LuebenOpenApiHttpTiggerContextOptions options, Func<VisitorCollection, TVisitor> factory)
            where TVisitor : IVisitor
        {
            var action = (VisitorCollection vc) =>
            {
                vc.Visitors.Add(factory(vc));
            };

            options.VisitorActions.Add(action);

            return options;
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddNamingStrategy(this LuebenOpenApiHttpTiggerContextOptions options, NamingStrategy namingStrategy)
        {
            if (namingStrategy is null)
            {
                throw new ArgumentNullException(nameof(namingStrategy));
            }

            options.NamingStrategy = namingStrategy;
            return options;
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddCommonHeader(this LuebenOpenApiHttpTiggerContextOptions options, string headerName, string description, bool required = false)
        {
            options.CommonOpenApiParameters.AddOpenApiParameter<string>(headerName, description, required, ParameterLocation.Header);

            return options;
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddDocumentFilter(
            this LuebenOpenApiHttpTiggerContextOptions options,
            Func<LuebenOpenApiHttpTiggerContextOptions, IDocumentFilter> documentFilterFactory)
        {
            if (documentFilterFactory is null)
            {
                throw new ArgumentNullException(nameof(documentFilterFactory));
            }

            return options.AddDocumentFilter(documentFilterFactory(options));
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddDocumentFilter<TDocumentFilter>(this LuebenOpenApiHttpTiggerContextOptions options)
            where TDocumentFilter : IDocumentFilter, new()
        {
            return options.AddDocumentFilter(new TDocumentFilter());
        }

        public static LuebenOpenApiHttpTiggerContextOptions AddDocumentFilter(this LuebenOpenApiHttpTiggerContextOptions options, IDocumentFilter documentFilter)
        {
            if (documentFilter is null)
            {
                throw new ArgumentNullException(nameof(documentFilter));
            }

            options.OpenApiConfigurationOptions.DocumentFilters.Add(documentFilter);
            return options;
        }
    }
}
