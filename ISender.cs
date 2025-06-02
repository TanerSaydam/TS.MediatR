using Microsoft.Extensions.DependencyInjection;

namespace TS.MediatR;

public interface ISender
{
    Task Send(IRequest request, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}

public sealed class Sender(
    IServiceProvider serviceProvider) : ISender
{
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var sp = serviceProvider;
        var interfaceType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        var pipelineType = typeof(IPipelineBehavior<>).MakeGenericType(request.GetType());

        RequestHandlerDelegate handlerDelete = () =>
        {
            var handler = sp.GetRequiredService(interfaceType);
            var method = interfaceType.GetMethod("Handle")!;
            return (Task)method.Invoke(handler, new object[] { request, cancellationToken })!;
        };

        var behaviors = (IEnumerable<object>)sp.GetServices(pipelineType);

        var pipeline = behaviors
            .Reverse()
            .Aggregate(
                handlerDelete,
                (next, behavior) =>
                {
                    return () =>
                    {
                        var method = pipelineType.GetMethod("Handle")!;
                        return (Task)method.Invoke(
                            behavior,
                            new object[] { request, next, cancellationToken })!;
                    };
                }
            );

        await pipeline();
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var sp = serviceProvider;
        var interfaceType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var pipelineType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));

        RequestHandlerDelegate<TResponse> handlerDelete = () =>
        {
            var handler = sp.GetRequiredService(interfaceType);
            var method = interfaceType.GetMethod("Handle")!;
            return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
        };

        var behaviors = (IEnumerable<object>)sp.GetServices(pipelineType);

        var pipeline = behaviors
            .Reverse()
            .Aggregate(
                handlerDelete,
                (next, behavior) =>
                {
                    return () =>
                    {
                        var method = pipelineType.GetMethod("Handle")!;
                        return (Task<TResponse>)method.Invoke(
                            behavior,
                            new object[] { request, next, cancellationToken })!;
                    };
                }
            );

        return await pipeline();
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        using var scoped = serviceProvider.CreateScope();
        var sp = scoped.ServiceProvider;

        var interfaceType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());

        var handlers = (IEnumerable<object>)sp.GetServices(interfaceType)!;

        var tasks = handlers
            .Select(handler =>
            {
                var method = interfaceType.GetMethod("Handle")!;
                return (Task)method.Invoke(handler, new object[] { notification, cancellationToken, })!;
            }).ToArray();

        await Task.WhenAll(tasks);
    }
}
