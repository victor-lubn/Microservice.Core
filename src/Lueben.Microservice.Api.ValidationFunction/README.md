# FunctionBase
Base class for function which want to validate request model with fluent validation.
If validation faoils it raise a specific type of exception which contains all fluent validation errors.

## Validation middleware
1. Register middleware in Program to handle validation related exceptions.

```csharp
...
app.UseMiddleware<ModelValidationExceptionMiddleware>();
...
```
2. Register middleware in Program to verify required SourceConsumer header is passed within the request.

```csharp
...
app.UseMiddleware<RequiredHeadersMiddleware>();
...
```