using System.Reflection;

namespace Willow.Registration;

// GUIDE_REQUIRED ASSEMBLY REGISTRATION explain why scoped can be escaped, and why its not a problem
/// <summary>
/// Implementations of which registered with the DI will all be called to define all the tasks required with loading
/// new assemblies (from plugins) into the system.
/// </summary>
/// <remarks>
/// Loaded via <see cref="IAssemblyRegistrar"/>
/// </remarks>
public interface IAssemblyRegistrar
{
    /// <summary>
    /// Types the system should add by default whenever adding a new assembly, those types are also considered shared
    /// types across assemblies.
    /// </summary>
    /// <remarks>
    /// All those service are defined as singletons and can be discovered either via the concrete type of the service or
    /// the generic types. <br/>
    /// Open generic types are discoverable by the defined generic type so
    /// <c>public class Example : IExample&lt;int&gt;</c> is discoverable both as <c>Example</c> and
    /// <c>IExample&lt;int&gt;</c>
    /// </remarks>
    public Type[] ExtensionTypes => [];

    /// <summary>
    /// Allows the registrar to define a start up function for the assembly, can return a <see cref="Task.CompletedTask"/>
    /// if not needed. <br/>
    /// This runs before the assembly is considered fully registered but after the <see cref="ExtensionTypes"/> have been
    /// resolved, allowing for pre-processing, or activating the registered functions.
    /// </summary>
    /// <param name="assembly">The assembly that is being registered now.</param>
    /// <param name="assemblyId">A unique assembly id.</param>
    /// <param name="serviceProvider">
    ///     A provider scoped to the assembly, meaning it only returns items registered with the assembly.
    /// </param>
    Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider);

    /// <summary>
    /// Allows the registrar to define a stopping function for the assembly, can return a <see cref="Task.CompletedTask"/>
    /// if not needed.
    /// This runs before the assembly unregisters the services defined in <see cref="IServiceRegistrar"/> and
    /// <see cref="ExtensionTypes"/> allowing for cleanup.
    /// </summary>
    /// <param name="assembly">The assembly that is being registered now.</param>
    /// <param name="assemblyId">A unique assembly id.</param>
    /// <param name="serviceProvider">
    ///     A provider scoped to the assembly, meaning it only returns items registered with the assembly.
    /// </param>
    Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider);
}
