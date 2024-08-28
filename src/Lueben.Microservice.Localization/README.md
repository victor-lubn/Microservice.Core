# Description

This package contains an localization filter and localization client for value of current culture.

## How to use:

1. Add to Program the usage of extension `AddLuebenLocalization()`.

2. Register in Program the middleware to handle passed local code with the request. 

```csharp
...
app.UseMiddleware<LocalizationMiddleware>();
...
```

3. Register in Program the middleware to handle LocaleCodeNotSupported exceptions. 

```csharp
...
app.UseMiddleware<LocaleCodeNotSupportedMiddleware>();
...
```

   Like that the current culture will be set for each request.
   `Localization.Current` is value of current `CultureInfo`.

```csharp
if (Lueben.Microservice.Localization.Localization.Current.Name == "en-GB")
{
    // some logic here
}
```

2. Add to configuration - `LocalizationOptions:AllowedLocales` as example = `"en-GB,en-IE,fr-FR,fr-BE"`.
   Also, if it is needed than `LocalizationOptions:DefaultLocale = "en-GB"`

3. When you add localization then the header `"Lueben-Locale-Code"` will be required or configuration `LocalizationOptions:DefaultLocale`

4. Localization middleware by default will be applied to all functions in the project
