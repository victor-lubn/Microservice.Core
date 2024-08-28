namespace Lueben.Microservice.HealthChecks.Models
{
    public class HealthCheck
    {
        public string Status { get; set; }

        public string Component { get; set; }

        public string Description { get; set; }

        public string Exception { get; set; }
    }
}
