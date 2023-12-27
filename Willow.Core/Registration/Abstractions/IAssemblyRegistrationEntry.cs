using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

public interface IAssemblyRegistrationEntry
{
    void RegisterAssemblies(Assembly[] assemblies);
}