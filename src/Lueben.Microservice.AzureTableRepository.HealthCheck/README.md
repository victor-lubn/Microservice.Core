# Description

This package provides a Health check service that checks whether the Azure table is available or not.

# Examples

In order to register event Azure table check you can use the following code:

```csharp
...
services
    .AddAzureTableHealthCheck<T>();
```