using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

/// <summary>
/// Registers with the DI all types found in the assemblies, they are all registered as singletons.
/// </summary>
internal interface IInterfaceRegistrar
{
    /// <inheritdoc cref="IInterfaceRegistrar" />
    /// <param name="typeToDeriveFrom">
    /// The type to be registered. <br/>
    /// all concrete implementations of the type will be registered.
    /// </param>
    /// <param name="assemblies">The assemblies to scan.</param>
    void RegisterDeriving(Type typeToDeriveFrom, Assembly[] assemblies);

    /// <summary>
    /// <see cref="RegisterDeriving" /> but the type is <typeparamref name="T" />,
    /// </summary>
    void RegisterDeriving<T>(Assembly[] assemblies);
}
