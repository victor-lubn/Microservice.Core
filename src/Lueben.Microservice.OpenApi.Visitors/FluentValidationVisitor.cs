using System;
using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors
{
    public class FluentValidationVisitor : TypeVisitor
    {
        private readonly IServiceProvider _serviceProvider;

        public FluentValidationVisitor(VisitorCollection visitorCollection, IServiceProvider serviceProvider)
            : base(visitorCollection)
        {
            _serviceProvider = serviceProvider;
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

            ApplyValidationRules(type, propertyInfo, schema);
        }

        private void ApplyValidationRules(KeyValuePair<string, Type> type, MemberInfo propertyInfo, OpenApiSchema schema)
        {
            var validator = GetValidator(propertyInfo.DeclaringType);
            if (validator is null)
            {
                return;
            }

            var validatorDescriptor = validator.CreateDescriptor();

            foreach (var group in validatorDescriptor.GetValidatorsForMember(propertyInfo.Name))
            {
                var propertyValidator = group.Validator;

                if (propertyValidator is INotEmptyValidator)
                {
                    schema.Required.Add(type.Key);
                }

                if (propertyValidator is ILengthValidator lengthValidator)
                {
                    if (lengthValidator.Max > 0)
                    {
                        schema.MaxLength = lengthValidator.Max;
                    }

                    if (lengthValidator.Min > 0)
                    {
                        schema.MinLength = lengthValidator.Min;
                    }
                }

                if (propertyValidator is ScalePrecisionValidator<decimal> precisionValidator)
                {
                    var scale = precisionValidator.Scale;
                    var precision = precisionValidator.Precision;
                    var intPart = precision > scale ? new string('9', precision - scale) : "0";
                    var fPart = new string('9', scale);
                    var maxValue = scale > 0 ? $"{intPart}.{fPart}" : intPart;
                    schema.Maximum = decimal.Parse(maxValue);
                }

                if (propertyValidator is IEmailValidator)
                {
                    schema.Example = new OpenApiString("example@domain.com");
                    schema.Format = "email";
                }

                if (propertyValidator is IComparisonValidator comparisonValidator
                    && comparisonValidator.Comparison == Comparison.GreaterThanOrEqual)
                {
                    try
                    {
                        var minimum = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                        schema.ExclusiveMinimum = true;
                        schema.Minimum = minimum;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (propertyValidator is IRegularExpressionValidator regexExpressionValidator)
                {
                    schema.Pattern = regexExpressionValidator.Expression;
                }
            }
        }

        private IValidator GetValidator(Type modelType)
        {
            var validatorType = typeof(AbstractValidator<>).MakeGenericType(modelType);

            var validator = (IValidator)_serviceProvider.GetService(validatorType);
            return validator;
        }
    }
}