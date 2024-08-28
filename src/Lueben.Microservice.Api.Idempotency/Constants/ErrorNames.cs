namespace Lueben.Microservice.Api.Idempotency.Constants
{
    public class ErrorNames
    {
        public const string IdempotencyPayloadMismatch = "IDEMPOTENCY_PAYLOAD_MISMATCH";

        public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";

        public const string IdempotencyNotValid = "IDEMPOTENCY_NOT_VALID";
    }
}
