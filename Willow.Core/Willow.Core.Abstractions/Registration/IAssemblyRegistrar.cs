using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Registration.Exceptions;

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
    /// Registers the assembly into the DI.
    /// </summary>
    /// <param name="assembly">The assembly that is being registered now.</param>
    /// <param name="assemblyId">A unique assembly id.</param>
    /// <param name="services">The service collection to add systems into.</param>
    /// <exception cref="RegistrationMustBeSingletonException">
    /// All the registrations with the system should be singletons, this is mainly due to the fact the system heavily
    /// relies on services that must be singletons and adding other levels of dependencies heavily complicates the
    /// dependency model.
    /// </exception>
    void Register(Assembly assembly, Guid assemblyId, IServiceCollection services);

    /// <summary>
    /// Allows the registrar to define a start up function for the assembly, can return a <see cref="Task.CompletedTask"/>
    /// if not needed. <br/>
    /// This runs before the assembly is considered fully registered but after the <see cref="Register"/> function,
    /// allowing for pre-processing, or activating the registered functions.
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
    /// This runs before the assembly unregisters the services defined in <see cref="Register"/> allowing for cleanup.
    /// </summary>
    /// <param name="assembly">The assembly that is being registered now.</param>
    /// <param name="assemblyId">A unique assembly id.</param>
    /// <param name="serviceProvider">
    ///     A provider scoped to the assembly, meaning it only returns items registered with the assembly.
    /// </param>
    Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider);
}
