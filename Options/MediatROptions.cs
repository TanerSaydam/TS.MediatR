using System.Reflection;

namespace TS.MediatR.Options;
public sealed class MediatROptions
{
    internal List<Assembly> Assemblies { get; set; } = new();
    internal List<Type> PipelineBehaviors { get; set; } = new();

    public void RegisterServicesFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
    }

    public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        Assemblies.AddRange(assemblies);
    }

    public void AddOpenBehavior(Type behaviorType)
    {
        PipelineBehaviors.Add(behaviorType);
    }
}
