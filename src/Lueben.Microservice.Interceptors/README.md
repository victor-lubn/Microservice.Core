# Description

The idea of having this package is to reuse retry and curcuit breaker logic for different clients without writing client specific decorators and without changing client interfaces or constuctors.
To do this Castle.DynamicProxy is used.

# Examples

Adding CB for eventhub client


```csharp
public static IServiceCollection AddEventHubClient(this IServiceCollection services)
{
   services.RegisterConfiguration<EventHubOptions>(nameof(EventHubOptions));
            
   return services
       .AddTransient(p =>
       {
          IEventDataSender target = new EventDataSender(p.GetService<IOptionsSnapshot<EventHubOptions>>(), p.GetService<ILogger<EventDataSender>>());
          var circuitBreakerInterceptor = new CircuitBreakerInterceptor(p.GetService<ICircuitBreakerClient>(), nameof(EventDataSender));
          var generator = new ProxyGenerator();
          return generator.CreateInterfaceProxyWithTarget(target, circuitBreakerInterceptor);
       });
}
```

Adding Retry for RestSharpClient

```csharp

...
var retryPolicyInterceptor = new RetryPolicyInterceptor<RestClientApiException>(retryPolicy ?? _retryPolicy, _loggerService, (ex) =>
{
     if (ex.StatusCode.HasValue && CheckStatusCode(ex.StatusCode.Value))
     {
          throw new RetriableOperationFailedException();
     }
});

return generator.CreateInterfaceProxyWithTarget(restClient, retryPolicyInterceptor);
...
```