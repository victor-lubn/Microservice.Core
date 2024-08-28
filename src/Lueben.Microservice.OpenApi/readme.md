# Open API for Azure Functions

Based on `Microsoft.Azure.WebJobs.Extensions.OpenApi` package

v3.0.0 Introduces fluent approach towards configuring open api for azure functions

## Sample

```csharp

services.AddOpenApi((serviceProvider, options) => 
{
	return options.UseOpenApiConfigurationFile("<config_file.json>")
		.AddCommonHeader("name", "description", required: false)
		.AddDocumentFilter<TDocumentFilter>()
		.AddVisitor<TVisitor>();
})

```