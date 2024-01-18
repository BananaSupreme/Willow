using System.Reflection;

namespace Willow.Core.Eventing.Abstractions;

/// <summary>
/// This is where the assembly scanning that registers events lies.<br/>
/// Registers <see cref="IEventHandler{TEvent}" /> with both DI and dispatcher.
/// </summary>
internal interface IEventRegistrar
{
    /// <summary>
    /// Register <see cref="IEventHandler{TEvent}" /> with both DI and dispatcher.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan.</param>
    void RegisterFromAssemblies(Assembly[] assemblies);
}
