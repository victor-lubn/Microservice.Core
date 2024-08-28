using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors
{
    public class AuditDateTimeFieldsVisitor : DateTimeTypeVisitor
    {
        private readonly List<string> _auditFields = new List<string> { "created", "updated" };

        public AuditDateTimeFieldsVisitor(VisitorCollection visitorCollection) : base(visitorCollection)
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

            if (_auditFields.Contains(type.Key.ToLower()))
            {
                schema.ReadOnly = true;
                schema.Description = "Audit field (is set automatically using UTC time).";
            }
        }
    }
}