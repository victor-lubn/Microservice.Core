# Description
Contains service collection extension to add retry policy to RestSharpClient for the predefined list of status codes:
  - TooManyRequests,
  - RequestTimeout,
  - Conflict,
  - BadGateway,
  - ServiceUnavailable,
  - GatewayTimeout


# Usage

```csharp
...
services
    .AddRestClientRetryPolicy();
```
