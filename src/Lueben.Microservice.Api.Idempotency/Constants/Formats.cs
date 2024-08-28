namespace Lueben.Microservice.Api.Idempotency.Constants
{
    public static class Formats
    {
        public const string UuidRegexFormat = @"^[0-9a-fA-F]{8}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{12}$";
        public const string AzureTableDateTimeFormat = "yyyy'-'MM'-'ddTHH':'mm':'ss'Z'";
    }
}
