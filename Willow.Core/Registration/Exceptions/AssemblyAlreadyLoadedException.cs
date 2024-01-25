namespace Willow.Core.Registration.Exceptions;

/// <summary>
/// The assembly was already loaded in the system, assemblies should only be loaded once or funky things happen.
/// </summary>
public sealed class AssemblyAlreadyLoadedException : InvalidOperationException
{
    public AssemblyAlreadyLoadedException() : base(
        "Tried to load the assembly twice, this can lead to unexpected issues.")
    {
    }
}
