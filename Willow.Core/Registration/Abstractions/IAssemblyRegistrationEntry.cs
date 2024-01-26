using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

/// <summary>
/// the assembly registration entry point, from here all <see cref="IAssemblyRegistrar" /> in the DI will be called.
/// </summary>
public interface IAssemblyRegistrationEntry
{
    /// <summary>
    /// Convenience method that registers multiple assemblies at once.
    /// </summary>
    /// <param name="assemblies">The assemblies to register.</param>
    /// <returns>All ids that can be used to unregister the assemblies from the system.</returns>
    Task<Guid[]> RegisterAssembliesAsync(Assembly[] assemblies);

    /// <summary>
    /// Adds the assembly into the system.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    /// <returns>An id that can be used to unregister the assembly from the system.</returns>
    Task<Guid> RegisterAssemblyAsync(Assembly assembly);

    /// <summary>
    /// Removes the assembly from the system.
    /// </summary>
    /// <param name="assemblyId">The assembly id given in the registration.</param>
    Task UnregisterAssemblyAsync(Guid assemblyId);
}
