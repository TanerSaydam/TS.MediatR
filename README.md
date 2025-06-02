# TS.MediatR

**TS.MediatR** is a lightweight and flexible CQRS/Mediator library for .NET. It supports `IRequest`, `INotification`, `IPipelineBehavior`, and works seamlessly with Dependency Injection.

## 🔧 Installation

Install via NuGet:

```dash
dotnet add package TS.MediatR
```

## 🚀 Getting Started

```csharp
using TS.MediatR;

services.AddMediatR(options =>
{
    options.AddRegisterAssemblies(typeof(Program).Assembly);
    options.AddOpenBehavior(typeof(LoggingBehavior<,>)); //with response
    options.AddOpenBehavior(typeof(ValidationBehavior<>)); //no response
});
```

## 🧩 IRequest / IRequestHandler

Define a request and its corresponding handler:

`With Response`

```csharp
public class GetUserQuery : IRequest<UserDto>
{
    public int Id { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = new UserDto { Id = request.Id, Name = "Jhon Doe" };
        return Task.FromResult(user);
    }
}
```

`No Response`

```csharp
public class CreateUserCommand : IRequest
{
    public string Name { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<CreateUserCommand>
{
    public Task Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = new UserDto { Name = request.Name };
        return Task.FromResult(user);
    }
}
```

## 📤 Sending with ISender

Use `ISender` to send the request:

`With Response`

```csharp
var result = await sender.Send(new GetUserQuery { Id = 1 });
Console.WriteLine(result.Name);
```

`No Response`

```csharp
await sender.Send(new CreateUserCommand { Name = "Jhon Doe" });
```

## 🔁 Pipeline Behavior

Use `IPipelineBehavior` for cross-cutting concerns like logging, validation, etc.

`With Response`

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequesteHandlerDelete<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Request received: {typeof(TRequest).Name}");
        var response = await next();
        Console.WriteLine($"Response returned: {typeof(TResponse).Name}");
        return response;
    }
}
```

`No Response`

```csharp
public class ValidationBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    public async Task Handle(TRequest request, RequesteHandlerDelete<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Request received: {typeof(TRequest).Name}");
        await next();
        Console.WriteLine($"Response returned: {typeof(TResponse).Name}");
        return response;
    }
}
```

## 📣 Notification Handling

Define a notification and its handler:

```csharp
public class ProductCreatedEvent : INotification
{
    public Guid Id { get; set; }
    public ProductCreatedEvent(Guid id)
    {
        Id = id;
    }
}

public class SendEmailHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Id sent: {notification.Id}");
        return Task.CompletedTask;
    }
}
```

Publish a notification:

```csharp
await sender.Publish(new ProductCreatedEvent(product.Id));
```

## 📦 Features

- ✅ Request/Response (`IRequest`, `IRequest<TResponse>`)
- ✅ Notifications (`INotification`)
- ✅ Pipeline behaviors (`IPipelineBehavior`)
- ✅ Dependency Injection ready
- ✅ Scoped resolution and handler chaining
- ✅ Fully async support

## 📁 License

Licensed under the MIT License.
