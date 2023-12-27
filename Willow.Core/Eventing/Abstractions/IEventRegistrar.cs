using System.Reflection;

namespace Willow.Core.Eventing.Abstractions;

public interface IEventRegistrar
{
    void RegisterFromAssemblies(Assembly[] assemblies);
}