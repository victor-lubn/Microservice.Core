namespace Lueben.Microservice.Api.Idempotency.Constants
{
    public static class ErrorMessages
    {
        public const string IdempotencyKeyNotValidError = "The idempotency key isn't in UUID format.";

        public const string IdempotencyKeyNullOrEmptyError = "The idempotency key is null or empty.";

        public const string ChangedPayloadError = "Previous and current payloads for this idempotency don't match. Provide a new idempotency key.";

        public const string IdempotencyConflictError = "Previous request with this idempotency still hasn't completed.";

        public const string EmptyBodyError = "The body cannot be empty.";
    }
}
