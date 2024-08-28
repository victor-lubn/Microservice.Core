namespace Lueben.Microservice.Api.Idempotency.Models
{
    public class IdempotencyEntity : Entity
    {
        public string FunctionName { get; set; }

        public string PayloadHash { get; set; }

        public string EntityType { get; set; }

        public string Response { get; set; }
    }
}
