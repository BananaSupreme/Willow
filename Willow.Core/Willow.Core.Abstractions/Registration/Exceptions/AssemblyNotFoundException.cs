namespace Willow.Registration.Exceptions;

/// <summary>
/// Assembly unregistration was requested but the assembly was never registered.
/// </summary>
public sealed class AssemblyNotFoundException : InvalidOperationException
{
    public AssemblyNotFoundException() : base("The assembly requested was not found.")
    {
        
    }
}
