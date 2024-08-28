# Description

Created as a workaround for issue with durable circuit breakers based on durable entities running on azure storage account.
https://github.com/Azure/azure-functions-durable-extension/issues/2469

The package contains time triggered job to periodically run and cleanup history of circuit breakers.

# Setup

## Schedule

Set CircuitBreakerCleanUpTimerScheduleExpression schedule.

```
"CircuitBreakerCleanUpTimerScheduleExpression": "0 0 1 * * *",
```

## Configuration

Configuration example

```
"CircuitBreakerCleanUpOptions:Ids:[0]": "CdcServiceClient",
"CircuitBreakerCleanUpOptions:Ids:[1]": "DpdClient",
"CircuitBreakerCleanUpOptions:PurgeWithoutAnalyze": "true"
```

## Ids. 
Provide them if there is a need to periodically purge only specific durable entity instances (keys).

### PurgeWithoutAnalyze.
By default only those entity instances are purged which has a history with link to blob files which are stored in large-messages container.
When PurgeWithoutAnalyze is set to true then entity instance history is not analyzed.

### EntityName 

If not provided then "durablecircuitbreaker" name will be used.

## Services registration

Configure services using "AddCircuitBreakerCleanUp" extension.

```
public override void Configure(IFunctionsHostBuilder builder)
{
      builder.Services.AddCircuitBreakerCleanUp();
}
```