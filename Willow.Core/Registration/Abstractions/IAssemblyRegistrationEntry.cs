using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

/// <summary>
/// the assembly registration entry point, from here all <see cref="IAssemblyRegistrar" /> in the DI will be called.
/// </summary>
public interface IAssemblyRegistrationEntry
{
    /// <summary>
    /// Entry point for assembly registration.
    /// </summary>
    /// <param name="assemblies">The assemblies to register.</param>
    void RegisterAssemblies(Assembly[] assemblies);
}
