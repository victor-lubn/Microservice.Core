namespace Lueben.Microservice.RestSharpClient.Factory
{
    public class RestSharpClientOptions
    {
        public string FunctionKey { get; set; }

        public string Scope { get; set; }

        public string ApiVersion { get; set; }

        public string CircuitBreakerId { get; set; }

        public bool EnableRetry { get; set; }
    }
}