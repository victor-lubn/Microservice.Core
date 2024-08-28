namespace Lueben.Microservice.Localization.Configurations
{
    public class LocalizationOptions
    {
        public static LocalizationOptions Value { get; set; }

        public string AllowedLocales { get; set; }

        public string DefaultLocale { get; set; }
    }
}
