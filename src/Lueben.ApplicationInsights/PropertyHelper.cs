namespace Lueben.ApplicationInsights
{
    public class PropertyHelper
    {
        public static string GetApplicationPropertyName(string property)
        {
            if (!property.StartsWith(Constants.CompanyPrefix))
            {
                return string.Join(Constants.Separator, Constants.CompanyPrefix, property);
            }

            return property;
        }

        public static string GetCustomDataPropertyName(string property)
        {
            if (!property.StartsWith(Constants.CompanyPrefix))
            {
                return string.Join(Constants.Separator, Constants.CompanyPrefix, Constants.DataPrefix, property);
            }

            return property;
        }
    }
}