namespace Lueben.Microservice.Api.Idempotency.Options
{
    public class IdempotencyAzureTableOptions
    {
        public string TableName { get; set; } = "Idempotency";
    }
}
