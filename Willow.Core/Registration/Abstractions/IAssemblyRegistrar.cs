using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

/// <summary>
/// Implementations of which registered with the DI will all be called to define all the tasks required with loading
/// new assemblies (from plugins) into the system.
/// </summary>
internal interface IAssemblyRegistrar
{
    /// <inheritdoc cref="IAssemblyRegistrar"/>
    /// <param name="assemblies">All the assemblies in the system.</param>
    void RegisterFromAssemblies(Assembly[] assemblies);
}