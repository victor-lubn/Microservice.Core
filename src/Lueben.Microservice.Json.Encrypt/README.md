# Description
This package contains an encryption converter and encryption contract resolver for Newtonsoft.Json.
Encrypt contract resolver creates encryption converter for fields with the JsonEncrypt attribute.

## How to use:
1. Add to Program - AddJsonEncryption. This will register Encryption options.

```csharp
...
services
    .AddJsonEncryption();
```

2. Register the following middleware in Program. This will change global JsonConvert.DefaultSettings.
EncryptedStringPropertyResolver will be set as ContractResolver.

```csharp
...
app.UseMiddleware<JsonEncryptingMiddleware>();
...
```

3. Set the [JsonEncrypt] attribute for fields that should be encrypted during serialization.

# Be aware
1. If you add encryption and function has old data without encryption, then the error will occur during decryption.
As an example, when you add messages to event hub without encription feature, but read with encription feature.
2. if we change ConractResolver in JsonConvert.DefaultSettings in PipelineFunction (by FunctionJsonSerializerSettingsProvider.CreateSerializerSettings) than the creation of EncryptingJsonConverter shoud be changed.