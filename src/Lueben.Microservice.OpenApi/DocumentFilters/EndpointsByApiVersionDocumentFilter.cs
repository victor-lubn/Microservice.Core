using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace Lueben.Microservice.OpenApi.DocumentFilters
{
    public class EndpointsByApiVersionDocumentFilter : IDocumentFilter
    {
        public const string ApiVersionQueryParameterName = "apiversion";
        private static readonly Regex VersionRegex = new Regex("[vV]([0-9]+)", RegexOptions.Compiled);

        public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
        {
            var apiVersion = req.Query[ApiVersionQueryParameterName];
            if (string.IsNullOrEmpty(apiVersion) || !IsVersionValid(apiVersion))
            {
                return;
            }

            foreach (var pathKey in document.Paths.Keys.Where(key => !key.StartsWith($"/{apiVersion}")))
            {
                document.Paths.Remove(pathKey);
            }
        }

        private bool IsVersionValid(string apiVersion)
        {
            var match = VersionRegex.Match(apiVersion);
            return match.Success;
        }
    }
}
