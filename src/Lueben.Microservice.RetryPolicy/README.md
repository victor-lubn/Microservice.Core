# Description

This package provides an implementation of retry logic.
There are two main classes in this package:
- *RetryPolicy* class that allows you to introduce the retry policy mechanism to your solution.  
- *RetryPolicyOptions* lets you define the maximum number of retries.
The package uses **Polly** library. To get more information about this library you can follow the [link](https://github.com/App-vNext/Polly)
Another important thing in this package is **ServiceCollectionExtensions** class with *AddRetryPolicy* method. This method registers retry-policy-related services to your solution.

# Examples

1. Use AddRetryPolicy extension method in your Startup file

```csharp
...
services
    .AddRetryPolicy();
```

2. Inject IRetryPolicy to your service

```csharp

...
public MyService(IRetryPolicy<T> retryPolicy)
{
    _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
}
...
```

3. Use Execute method of IRetryPolicy

```csharp

...
await _retryPolicy.Execute(async (context) =>
{
    capture.Invoke();
    retryCount = (int)context[RetryPolicyConstants.RetryCountPropertyName];
    var task = (Task)invocation.ReturnValue;
    await task;
});
```