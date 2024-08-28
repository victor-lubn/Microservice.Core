using Lueben.Microservice.Localization.Configurations;

namespace Lueben.Microservice.Localization.Constants
{
    public static class ErrorMessages
    {
        public const string LocaleCodeNotSupported = "Locale Code: {0} not supported.";

        public static readonly string LocaleCodeHeaderNotFound =
            $"Missing required request header {CommonConstants.LocaleCodeHeaderKey} or missing configuration option {nameof(LocalizationOptions)}:{nameof(LocalizationOptions.DefaultLocale)}";
    }
}
