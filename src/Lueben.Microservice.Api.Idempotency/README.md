# Description
This package contains an idempotency provider and idempotency function wrapper.
The wrapper uses a provider to perform an idempotency checking (request conflict checking, payload checking, etc.).

## How to use:
1. Add to Program - AddAzureTableIdempotency. This will add AzureTableIdempotencyDataProvider,
IdempotencyFunctionWrapper. 

```csharp
...
services
    .AddAzureTableIdempotency();
```

2. Register in Program middleware to handle idemportency related exceptions. 

```csharp
...
app.UseMiddleware<HttpContextSetupMiddleware>();
app.UseMiddleware<IdempotencyExceptionMiddleware>();
```

3. Inject IFunctionWrapper in a function class and use a method of wrapper 'Execute' in the function class method,
where you need to add an idempotency. Use delegate 'azureFunction' for business logic.