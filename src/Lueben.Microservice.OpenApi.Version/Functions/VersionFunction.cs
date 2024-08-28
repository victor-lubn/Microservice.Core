using System.Reflection;
using Lueben.Microservice.OpenApi.Version.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Lueben.Microservice.OpenApi.Version.Functions
{
    public class VersionFunction
    {
        private readonly IVersionService _versionService;
        private readonly IVersionedMethodsQuery _versionsQuery;

        public VersionFunction(IVersionService versionService, IVersionedMethodsQuery versionsQuery)
        {
            _versionService = versionService;
            _versionsQuery = versionsQuery;
        }

        [FunctionName("version")]
        public IActionResult GetVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequest request,
            ILogger log)
        {
            var methods = _versionsQuery.Get(Assembly.GetExecutingAssembly());

            log.Log(LogLevel.Information, "Received get version request");

            return new JsonResult(new { Version = _versionService.GetVersion(methods) });
        }
    }
}
