namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines the relevant activation method for the <see cref="IVoiceCommand" />
/// </summary>
/// <remarks>
/// Not setting this will resort it to become the default, command mode, to make a command apply to all modes set
/// explicitly to null. <br/>
/// If a command is valid across multiple modes, multiple activations can be given and a cross of all tags and modes
/// would be created.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ActivationModeAttribute : Attribute, IVoiceCommandDescriptor
{
    public ActivationModeAttribute(string? activationMode)
    {
        ActivationModes = activationMode is not null ? [activationMode] : null;
    }

    public ActivationModeAttribute(params string[] activationModes)
    {
        ActivationModes = activationModes;
    }

    /// <summary>
    /// Selected activation mode.
    /// </summary>
    public string[]? ActivationModes { get; }
}
