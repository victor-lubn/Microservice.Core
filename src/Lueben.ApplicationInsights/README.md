# Description

Package to support Lueben tracing standard
https://dev.azure.com/LuebenJoinery/Lueben/_wiki/wikis/Lueben.wiki/576/MS-Tracing-and-Events-Standard?anchor=tracing-(optional)

It can be added to any type of solution - not only to azure functions but to app services, for example.
That is why it does not have "Microservice" in the package name.

## ApplicationTelemetryInitializer

It's purpose is to add standard properties to app insight telemetry entries.

## PropertyHelper

It's methods can be used to have standard Lueben prefixes for telemetry custom property names.