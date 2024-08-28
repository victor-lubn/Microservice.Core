# Description

This package provides a durable implementation of **Circuit breaker** pattern.
The implementation of Circuit breaker uses **Durable Entity functions** to persist circuit state durably accross different function apps.
This implementation is based on the original Polly implementation of Circuit breaker pattern. 
To get more information about Durable Entity functions you can follow the [link](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-entities?tabs=csharp)
In order to get more information about the original Polly implementation of Circuit breaker patter you can follow the [link](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker)

# Examples

1. Register Circuit-breaker-related services

```csharp
public static IServiceCollection AddCircuitBreaker(this IServiceCollection services, IList<string> circuitBreakerInstanceIds = null)
{
    services.RegisterHandlerNamedOptions<CircuitBreakerSettings>(circuitBreakerInstanceIds);

    return services.AddMemoryCache()
       .AddSingleton(typeof(OptionsManager<CircuitBreakerSettings>))
       .AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>()
       .AddSingleton<IPolicyRegistry<string>>(new PolicyRegistry())
       .AddSingleton<IDurableCircuitBreakerClient, DurableCircuitBreakerClient>()
       .AddSingleton<ICircuitBreakerClient, CircuitBreakerClient>()
       .AddDurableClientFactory()
       .AddSingleton<ICircuitBreakerStateChecker>(provider =>
       {
           return new CircuitBreakerStateChecker(provider.GetService<IDurableClientFactory>(),
                   provider.GetService<IDurableCircuitBreakerClient>(),
                   provider.GetService<ILogger<CircuitBreakerStateChecker>>(), 
                   provider.GetService<IConfiguration>(), 
                   provider.GetService<IOptions<DurableTaskOptions>>(),
                   circuitBreakerInstanceIds);
       });
}

```

2. Inject ICircuitBreakerClient to your service

```csharp

...
public MyService(ICircuitBreakerClient circuitBreakerClient)
{
    _circuitBreakerClient = circuitBreakerClient ?? throw new ArgumentNullException(nameof(circuitBreakerClient));
}
...
```

3. Use Execute method of ICircuitBreakerClient

```csharp

...
_circuitBreakerClient.Execute(_circuitBreakerId, async () =>
{
    capture.Invoke();
    var task = (Task)invocation.ReturnValue;
    await task;
}, () => throw new CircuitBreakerOpenStateException());
```