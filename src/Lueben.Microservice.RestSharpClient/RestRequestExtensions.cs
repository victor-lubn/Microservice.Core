using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;

namespace Lueben.Microservice.RestSharpClient
{
    public static class RestRequestExtensions
    {
        private const string UrlSegmentsRegexPattern = @"\{([A-Za-z]+)\}";
        private static readonly Regex SegmentsRegex = new(UrlSegmentsRegexPattern, RegexOptions.Compiled);

        public static void PopulateQueryStringParameters(this RestRequest request, object queryParameters)
        {
            if (queryParameters == null)
            {
                return;
            }

            var properties = new Dictionary<string, string>();
            queryParameters.GetType().GetProperties()
                .Where(property => property.GetValue(queryParameters, null) != null)
                .ToList()
                .ForEach(prop => properties.Add(prop.Name, prop.GetValue(queryParameters, null).ToString()));

            foreach (var property in properties)
            {
                request.AddQueryParameter(property.Key, property.Value);
            }
        }

        public static void PopulateUrlSegmentParameters(this RestRequest request, string url, IReadOnlyList<object> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return;
            }

            var matches = SegmentsRegex.Matches(url);
            if (parameters.Count != matches.Count)
            {
                throw new Exception($"Numbers of passed parameters {parameters.Count} doesn't match number of url segment parameters '{matches.Count}' of url {request.Resource}.");
            }

            for (var i = 0; i < matches.Count; i++)
            {
                request.AddUrlSegment(matches[i].Groups[1].Value, parameters[i].ToString());
            }
        }

        public static void PopulateBody(this RestRequest request, object body)
        {
            if (body == null)
            {
                return;
            }

            request.AddJsonBody(body);
        }

        public static void PopulateHeaders(this RestRequest request, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var headersCollection = headers?.ToList();
            if (headersCollection == null || !headersCollection.Any())
            {
                return;
            }

            request.AddHeaders(headersCollection);
        }
    }
}
