using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.OpenApi.Models;

namespace Lueben.Microservice.OpenApi.Extensions
{
    public static class OpenApiParameterExtensions
    {
        public static IList<OpenApiParameter> AddOpenApiParameter<T>(this IList<OpenApiParameter> parameters, string name, string description = null, bool required = false, ParameterLocation @in = ParameterLocation.Query)
        {
            return parameters.AddOpenApiParameter(typeof(T), name, description, required, @in);
        }

        public static IList<OpenApiParameter> AddOpenApiParameter(this IList<OpenApiParameter> parameters, Type type, string name, string description = null, bool required = false, ParameterLocation @in = ParameterLocation.Query)
        {
            parameters.ThrowIfNullOrDefault();
            type.ThrowIfNullOrDefault();
            name.ThrowIfNullOrWhiteSpace();
            OpenApiSchema schema = new OpenApiSchema
            {
                Type = type.ToDataType(),
                Format = type.ToDataFormat()
            };
            OpenApiParameter item = new OpenApiParameter
            {
                Name = name,
                Description = description,
                Required = required,
                In = @in,
                Schema = schema
            };
            parameters.Add(item);
            return parameters;
        }
    }
}
