using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors
{
    public class ComplexObjectVisitor : ObjectTypeVisitor
    {
        public ComplexObjectVisitor(VisitorCollection visitorCollection) : base(visitorCollection)
        {
        }

        public override void Visit(IAcceptor acceptor, KeyValuePair<string, Type> type, NamingStrategy namingStrategy, params Attribute[] attributes)
        {
            base.Visit(acceptor, type, namingStrategy, attributes);

            if (!(acceptor is OpenApiSchemaAcceptor instance))
            {
                return;
            }

            if (!instance.Schemas.TryGetValue(type.Key, out var schema))
            {
                return;
            }

            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                schema.OneOf = new List<OpenApiSchema> { new OpenApiSchema { Reference = schema.Reference }, new OpenApiSchema { Type = "null" } };
                schema.Reference = null;
            }
        }
    }
}