using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lueben.Microservice.OpenApi.Version.Abstractions;

namespace Lueben.Microservice.OpenApi.Version
{
    public class OpenApiVersionOperationFilter : IOpenApiOperationFilter
    {
        private static readonly Regex VersionRegex = new Regex("[vV]([0-9]+)", RegexOptions.Compiled);
        private readonly int _version;

        public OpenApiVersionOperationFilter(string apiVersion)
        {
            if (string.IsNullOrEmpty(apiVersion))
            {
                throw new ArgumentNullException(nameof(apiVersion));
            }

            _version = GetVersion(apiVersion);

            if (_version == 0)
            {
                throw new Exception($"Requested version {apiVersion} doesn't follow version pattern.");
            }
        }

        public IEnumerable<MethodInfo> Filter(IEnumerable<MethodInfo> functionMethods)
        {
            var methodGroups = functionMethods
                .Select(method =>
                {
                    var versionPart = method.DeclaringType?.Namespace?.Split('.').LastOrDefault();
                    var version = GetVersion(versionPart);
                    return new { method, version };
                })
                .Where(fi => fi.version > 0 && fi.version <= _version)
                .GroupBy(fi => fi.method);

            foreach (var methodVersions in methodGroups)
            {
                var max = methodVersions.Max(m => m.version);
                yield return methodVersions.First(m => m.version == max).method;
            }
        }

        private static int GetVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                return 0;
            }

            var match = VersionRegex.Match(versionString);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }
}