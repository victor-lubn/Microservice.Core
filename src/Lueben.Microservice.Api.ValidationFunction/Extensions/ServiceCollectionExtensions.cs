using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Lueben.Microservice.Api.ValidationFunction.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services, IEnumerable<Assembly> assemblies = null, Predicate<Type> typePredicate = null)
        {
            assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

            var validators = assemblies.SelectMany(s => s.GetTypes())
                .Where(v => v.BaseType != null && v.BaseType.IsAbstract && v.BaseType.Name.Contains(nameof(AbstractValidator<object>)) &&
                            (typePredicate?.Invoke(v) ?? true)).ToList();

            foreach (var validator in validators)
            {
                if (validator != null)
                {
                    services.AddTransient(validator.BaseType, validator);
                }
            }

            return services;
        }
    }
}