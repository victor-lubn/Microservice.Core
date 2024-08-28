# Description
Package for adding telemetry initializers for changing tracing of exceptions.

## ExceptionLowLevelTelemetryInitializer 
TelemetryInitializer is used to low the severity levels of some exceptions (and they derived exceptions)
which names are configured in ExceptionsLowLevelLogOptions:Exceptions configuration.

## ExceptionsLowLevelLogOptions 
Class for configuring options for ExceptionLowLevelTelemetryInitializer.
Exceptions field is the names of exceptions which severity level should be changed from Error to Low level.
Example in config :
    "ExceptionsLowLevelLogOptions:Exceptions:Information:[0]": "Test.ModelNotValidException",
    "ExceptionsLowLevelLogOptions:Exceptions:Information:[1]": "Test.AnotherModelNotValidException",
    "ExceptionsLowLevelLogOptions:Exceptions:Warning:[0]": "EntityNotFoundException"
    OR
    "ExceptionsLowLevelLogOptions:Exceptions:1:[0]": "Test.ModelNotValidException",
    "ExceptionsLowLevelLogOptions:Exceptions:1:[1]": "Test.AnotherModelNotValidException",
    "ExceptionsLowLevelLogOptions:Exceptions:0:[0]": "EntityNotFoundException"