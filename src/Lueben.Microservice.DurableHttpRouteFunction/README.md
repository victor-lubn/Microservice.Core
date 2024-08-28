# Introduction 
Example of the event handler which routes event payload to configured azure function.
It used durable functions to guarantee that event data is delivered.
Additional routes can be configured and no need to redeploy orchestrator.

# HttpEventHandlerOptions
Named options contains 3 properties for each route:
- ServiceUrl  - url to the azure functions
- FunctionKey - should be set if the function is protected with function key
- HealthcheckUrl - specify endpoint to check availabilitu of the function.

# DurableHttpRouteFunction

It contains orchestrator function which calls "subscriber" function.
If http call fails it retries http call after configured period of time.
Retry happens infinitly if limit is not set in the configuration.

# DurableHttpRouteEventHandler

Example of the event handler.
It executes orchestration if there are routing settings for event type.
NB! configuration is case sensetive!
