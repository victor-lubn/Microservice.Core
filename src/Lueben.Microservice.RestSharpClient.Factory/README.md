# Description
This package contains an example of RestSharpClientFactory which we use in microservices
which does http requests to APIM or directrly to azure functions and use Circuit Breakes and Retry logic.

At the same time it shows an example how to combine reusable functionality from other independent
packages to add required functionality to RestSharp http clients.

## Client Configuration

This factory uses Named Options to get configuration for each client.
Factory receives only reference to the object whcoih wants to create an instance of rest client.

This factory example uses type name of the passed object to find corresponding configuration.
This solution is used to avoid to mix cross cutting concerns to the client options.



Example of configuration for APIM scenario

```json
"RestSharpClientOptions:Service1:Scope" : "KeyVault:https://kvt-apim-{environment}-uks.vault.azure.net/secrets/kvs-sp-api-host-{environment}-scope"
"RestSharpClientOptions:Service1:ApiVersion" : "v1"
```

Example of configuration for FunctionKey authentication

```json
"RestSharpClientOptions:Service2:FunctionKey" : "KeyVault:https://kvt-oaa-{environment}-uks.vault.azure.net/secrets/kvs-oaa-email-host-key"
```

Example of adding interceptors

```json

"RestSharpClientOptions:Service1:CircuitBreakerId" : "Service1"
"RestSharpClientOptions:Service1:EnableRetry" : "true"

```


## Simple factory example

If there is only one RestClient in application and there is a requirement to
make call throught APIM then a factory can look as simple as below without
using named options, without CB and Retry interceptors.


```
    public class ApimRestSharpClientFactory : RestSharpClientFactory
    {
        private readonly IOptionsSnapshot<ApimClientOptions> _options;
        private readonly IServiceApiAuthorizer _authorizer;

        public ApimRestSharpClientFactory(ILoggerFactory loggerFactory,
            IOptionsSnapshot<ApimClientOptions> options,
            IServiceApiAuthorizer authorizer) : base(loggerFactory)
        {
            _options = options;
            _authorizer = authorizer;
        }

        public override IRestSharpClient Create(object client)
        {
            var restClient = base.Create(client);
            restClient.AddApimHeaders(_options.Value, _authorizer);

            restClient.SetSerializerSettings(FunctionJsonSerializerSettingsProvider.CreateSerializerSettings());
            return restClient;
        }
    }
```