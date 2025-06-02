namespace TS.MediatR;
public interface IRequest { }
public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest> where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public delegate Task RequestHandlerDelegate();
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    Task Handle(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken = default
        );
}

public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
        );
}

public interface INotification { }
public interface INotificationHandler<TNotification> where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}