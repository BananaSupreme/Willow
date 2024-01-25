namespace Willow.Core.Registration.Abstractions;

/// <summary>
/// Allows the user to access a variable amount of type <typeparamref name="T"/> when calling from DI, this basically
/// avoids the problem that happens when all the registrations are singletons, that when calling IEnumerable of a service,
/// it saves the services that were called.
/// </summary>
/// <typeparam name="T">Type of service to enumerate.</typeparam>
public interface ICollectionProvider<out T>
{
    /// <summary>
    /// Gets all the services of type <typeparamref name="T"/> currently registered in the DI.
    /// </summary>
    /// <returns>All the services of type <typeparamref name="T"/> currently registered in the DI.</returns>
    IEnumerable<T> Get();
}
