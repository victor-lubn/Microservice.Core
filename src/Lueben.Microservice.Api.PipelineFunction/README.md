# PipelineFunction
Base class for functions which need to execute common logic before and after a request 
or to handle exceptions.
Scopes execution of invocation and exception filters which are inherited from corresponding base classes 
only for descendant functions.


# PipelineFunctionsStartup
Splits dependency configuration into 2 parts: services and filters.