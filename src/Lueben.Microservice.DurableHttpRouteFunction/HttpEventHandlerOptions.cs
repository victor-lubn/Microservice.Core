namespace Lueben.Microservice.DurableHttpRouteFunction
{
    public class HttpEventHandlerOptions
    {
        public string ServiceUrl { get; set; }

        public string FunctionKey { get; set; }

        public string HealthCheckUrl { get; set; }
    }
}