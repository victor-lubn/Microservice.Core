# ExceptionHandlingMiddleware
Base class for functions middleware that need to handle exceptions.


# EntityExceptionMiddleware
Common impelentation of base ExceptionHandlingMiddleware to handle EntityNotFound exceptions

# EntityExceptionMiddleware, DefaultExceptionMiddleware
Common impelentation of base ExceptionHandlingMiddleware to handle unexpected exceptions

# Example of registration

 app.UseMiddleware<DefaultExceptionMiddleware>();
 app.UseMiddleware<EntityExceptionMiddleware>();

 NOTE: The first middleware received the request, updated it as needed, and then gave control to the next middleware. Similarly, the first middleware executes at last while processing the response. That's why it's important to have error handlers (Exception handler middleware) first in the process they can catch any problems and show them in a way that's easy for the user to understand.