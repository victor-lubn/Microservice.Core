using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Lueben.Microservice.Api.ValidationFunction.Extensions
{
    public static class TypeExtensions
    {
        public static string[] GetFilledProperties<T>(this T value)
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(value), $"{nameof(value)} cannot be null.");
            }

            bool PropertyHasValue(PropertyInfo prop)
            {
                if (prop.GetValue(value) is IList list && list.Count == 0)
                {
                    return false;
                }

                if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                {
                    return prop.GetValue(value) != null;
                }

                return prop.GetValue(value) == null
                    ? false
                    : !IsNullOrEmpty(Cast(prop.GetValue(value), prop.GetValue(value).GetType()));
            }

            return value.GetType()
                .GetProperties()
                .Where(PropertyHasValue)
                .Select(p => p.Name)
                .ToArray();
        }

        private static bool IsNullOrEmpty<T>(this T value)
        {
            if (typeof(T) == typeof(string))
            {
                return string.IsNullOrEmpty(value as string);
            }

            return value == null || value.Equals(default(T));
        }

        private static dynamic Cast(dynamic dynamicObject, Type castTo)
        {
            return Convert.ChangeType(dynamicObject, castTo);
        }
    }
}
