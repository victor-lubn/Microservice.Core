using System;
using System.Collections.Generic;
using System.Reflection;
using Lueben.Microservice.Serialization.Converters;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors
{
    public class ShortDateVisitor : DateTimeTypeVisitor
    {
        public ShortDateVisitor(VisitorCollection visitorCollection) : base(visitorCollection)
        {
        }

        public override bool IsVisitable(Type type)
        {
            return true;
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

            if (!instance.Properties.TryGetValue(type.Key, out var propertyInfo))
            {
                return;
            }

            var converterAttr = propertyInfo.GetCustomAttribute<JsonConverterAttribute>(false);
            if (converterAttr != null && converterAttr.ConverterType == typeof(ShortDateConverter))
            {
                schema.Example = new OpenApiString(new DateTime(2000, 1, 1).ToString(ShortDateConverter.Format));
                schema.Format = "date";
            }
        }
    }
}