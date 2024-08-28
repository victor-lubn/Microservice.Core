# Description

This package provides a service that helps you interact with Event hub.
This package consists of the **EventDataSender** that allows you to send events to Azure Event hub as well as options that have some properties:
* Namespace - Event hub namespace,
* Name - the name of the Event hub,
* ConnectionString - the connection string to Event hub,
* MaxRetryCount - the maximum number of retries,
* MaxRetryDelay - the maximum delay between retries.

The package uses standard **Azure.Messaging.EventHubs** nuget packes provided by Microsoft.
To get more information about this package you can follow the [link](https://docs.microsoft.com/en-us/samples/azure/azure-sdk-for-net/azuremessagingeventhubs-samples/)


# Examples

1. Register required services in your Startup file


```csharp
...
services.RegisterConfiguration<EventHubOptions>(nameof(EventHubOptions));
    .AddTransient<IEventDataSender, EventDataSender>()
```

2. Inject IEventDataSender to your service

```csharp

...
public MyService(IEventDataSender eventDataSender)
{
    _eventDataSender = eventDataSender;
}
...
```

3. Use SendEventAsync method of IEventDataSender

```csharp

...
await _sender.SendEventAsync(appData.CreateEvent());
```