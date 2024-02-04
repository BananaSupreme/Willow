namespace Willow.Registration.Exceptions;

/// <summary>
/// Services should only be registered as singletons in the system.
/// </summary>
public sealed class RegistrationMustBeSingletonException : InvalidOperationException
{
    public RegistrationMustBeSingletonException() : base("All registrations with the container should be singleton.")
    {
    }
}
