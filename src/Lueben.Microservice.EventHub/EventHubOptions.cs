namespace Lueben.Microservice.EventHub
{
    public class EventHubOptions
    {
        public string Namespace { get; set; }

        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public int? MaxRetryCount { get; set; }

        public int? MaxRetryDelay { get; set; }
    }
}