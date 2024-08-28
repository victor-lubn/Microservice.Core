using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lueben.Microservice.OpenApi.Version.Abstractions;

namespace Lueben.Microservice.OpenApi.Version
{
    public class VersionService : IVersionService
    {
        private const string FirstVersion = "v1";
        private readonly Regex _versionRegex = new Regex(@"v(\d+)");

        public string GetVersion(IList<MethodInfo> methods)
        {
            var captures = new List<string>();
            var methodInfos = methods ?? new List<MethodInfo>();

            foreach (var method in methodInfos)
            {
                Match match = _versionRegex.Match(method.DeclaringType?.FullName ?? throw new InvalidOperationException());

                if (match.Success)
                {
                    captures.Add(match.Captures[0].Value);
                }
            }

            captures = captures.OrderBy(x => x).ToList();

            return captures.Count > 0 ? captures.Last() : FirstVersion;
        }
    }
}
