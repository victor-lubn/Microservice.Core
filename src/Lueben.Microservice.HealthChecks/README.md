# Description

This package provides an Azure function implementation of health checks.
The main part of the health check logic in this package - **Health check Azure function**. 
This function is an HTTP-triggered function and it shows whether all the health check are passed or not.  
The package uses standard **Microsoft.Extensions.Diagnostics.HealthChecks** nuget packes provided by Microsoft.
To get more information about this package you can follow the [link](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-5.0)
Another important thing in this package is **HealthCheckServiceFunctionExtension** class with *AddFunctionHealthChecks* method. 
This method lets you register some preferred health checks to your solution.

# Examples

In order to register event hub health check and API health check you can use the following code:

```csharp
...
services
    .AddFunctionHealthChecks()
    .AddEventHubHealthCheck()
    .AddApiHealthCheck(apiHealthCheckUrl);
```