namespace Lueben.Microservice.Api.ValidationFunction.Constants
{
    public static class ErrorMessages
    {
        public const string ModelNotValidError = "Model is not valid.";

        public const string ModelDoesNotHaveAnyPropertiesError = "Model should have at least one property.";

        public const string EmptyBodyError = "The body cannot be empty.";
    }
}