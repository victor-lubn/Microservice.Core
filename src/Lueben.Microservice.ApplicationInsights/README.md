# Description
This package is specific to Azure Functions.

## CustomDataPropertyTelemetryInitializer 
Azure function has OOTB custom logging provider 
which adds all properties added by ILogger.BeginScope as custom properties to telemetry.
TelemetryInitializer implemented in this package is used to rename azure functions telemetry custom properties 
which starts with "prop__" prefix to "HWN_Data" prefixes" for a defined list of properties which are of most interest for 
troubleshooting like, for example, "applicationId", "orderId" etc.

## ILoggerService
Used to create AppInsight custom events as this functionality does not exists OOTB in azure functions.

## How to use

```csharp

services.AddApplicationInsightsTelemetry(options => 
{
	options.AddTelemetryInitializer<TelemetryInitializer1>()
		.AddTelemetryInitializer<TelemetryInitializer1>(serviceProvider => new TelemetryInitializer2(serviceProvider.GetService<SomeService>(), p1)
})

```