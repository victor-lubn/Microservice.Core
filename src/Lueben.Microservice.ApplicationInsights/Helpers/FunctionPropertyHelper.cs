namespace Lueben.Microservice.ApplicationInsights.Helpers
{
    public class FunctionPropertyHelper
    {
        public const string FunctionCustomPropertyPrefix = "prop__";

        public static string GetOriginalPropertyName(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return property;
            }

            if (property.StartsWith(FunctionCustomPropertyPrefix))
            {
                return property.Substring(FunctionCustomPropertyPrefix.Length);
            }

            return property;
        }
    }
}