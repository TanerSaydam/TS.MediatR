using Microsoft.Extensions.DependencyInjection;
using TS.MediatR.Options;

namespace TS.MediatR;

public static class ServiceRegistrar
{
    public static IServiceCollection AddMediatR(
        this IServiceCollection services,
        Action<MediatROptions> options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));

        var config = new MediatROptions();
        options(config);

        #region IReuqestHandler
        foreach (var assembly in config.Assemblies)
        {
            var types = assembly.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract);

            var handlerTypes = types.SelectMany(t => t
                .GetInterfaces()
                .Where(i => i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                || i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                ))
                .Select(s => new { Interface = s, Impletation = t })
                );

            services.AddScoped<ISender, Sender>();

            foreach (var item in handlerTypes)
            {
                services.AddScoped(item.Interface, item.Impletation);
            }
        }
        #endregion

        #region IPipelineBehavior
        foreach (var pipeline in config.PipelineBehaviors)
        {
            var genericArg = pipeline.GetGenericArguments().Length;

            if (genericArg == 1)
            {
                services.AddScoped(typeof(IPipelineBehavior<>), pipeline);
            }
            else if (genericArg == 2)
            {
                services.AddScoped(typeof(IPipelineBehavior<,>), pipeline);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(genericArg));
            }
        }
        #endregion

        #region INotificationHandler
        foreach (var assembly in config.Assemblies)
        {
            var types = assembly.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract);

            var handlerTypes = types
                .SelectMany(t => t
                    .GetInterfaces()
                    .Where(t => t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
                    .Select(s => new { Interface = s, Implementation = t }));

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.Interface, handler.Implementation);
            }
        }
        #endregion

        return services;
    }
}