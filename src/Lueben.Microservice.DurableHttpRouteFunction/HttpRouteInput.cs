namespace Lueben.Microservice.DurableHttpRouteFunction
{
    public class HttpRouteInput
    {
        public string Payload { get; set; }

        public HttpEventHandlerOptions HandlerOptions { get; set; }
    }
}