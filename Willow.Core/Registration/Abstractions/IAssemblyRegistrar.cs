using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

public interface IAssemblyRegistrar
{
    void RegisterFromAssemblies(Assembly[] assemblies);
}