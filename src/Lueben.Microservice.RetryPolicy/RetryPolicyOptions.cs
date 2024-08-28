namespace Lueben.Microservice.RetryPolicy
{
    public class RetryPolicyOptions
    {
        public int MaxRetryCount { get; set; } = 3;
    }
}