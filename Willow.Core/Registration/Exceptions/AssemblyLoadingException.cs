namespace Willow.Core.Registration.Exceptions;

/// <summary>
/// Some error happened while loading or unloading the assembly, check inner exception.
/// </summary>
public sealed class AssemblyLoadingException : Exception
{
    public AssemblyLoadingException(Exception innerException) : base(
        "An inner exception was encountered while processing the assembly.", innerException)
    {
    }
}
