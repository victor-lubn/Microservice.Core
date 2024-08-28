# Description

This package provides a custom implementation of Mediator pattern. This pattern lets you decouple the logic of sending messages from the logic of handling them.
This package consists of the **Mediator** class as well as **ServiceCollectionExtensions** class with extension method *AddMediatr* allowing you to register all handlers in a given assembly.

MediatR has two kinds of messages it dispatches:
- Request/response messages (dispatched to a single handler),
- Notification messages (dispatched to multiple handlers).

# Examples

## Request/response

1. Create a request


```csharp
public class MyRequest : IRequest<string> { }
```

2. Create a handler

```csharp
public class MyHandler : IRequestHandler<MyRequest, string>
{
    public Task<string> Handle(MyRequest request, CancellationToken cancellationToken)
    {
        ...
    }
}
```

3. Send a message through the mediator

```csharp
...
var response = await mediator.Send(new MyRequest());
...
```

## Notifications

1. Create a notification message

```csharp
public class MyNotification : INotification { }
```

2. Create some handlers for your notification

```csharp
public class MyNotificationHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken)
    {
        ...
    }
}
```

3. Publish your message via the Mediator

```csharp
...
await mediator.Publish(new MyNotification());
...
```