# Description

This package provides a Health check service that checks whether the event hub is available or not.
This package consists of the service itself as well as options that have some properties:
* Namespace - Event hub namespace,
* Name - the name of the Event hub,
* ConnectionString - the connection string to Event hub,
* MaxRetryCount - the maximum number of retries,
* MaxRetryDelay - the maximum delay between retries.

The package also includes **EventHubHealthCheckBuilderExtensions** class with extension method *AddEventHubHealthCheck*. This extension method lets you register Event hub health check to your solution.