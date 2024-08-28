namespace Lueben.Microservice.Api.Models
{
    public class ValidationError
    {
        public string Field { get; set; }

        public string Value { get; set; }

        public string Issue { get; set; }

        public string Location { get; set; }
    }
}