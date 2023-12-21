using Willow.Core.Environment.Enums;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Attributes;

public class ActivationModeAttribute : Attribute
{
    public ActivationMode ActivationMode { get; }

    public ActivationModeAttribute(ActivationMode activationMode)
    {
        ActivationMode = activationMode;
    }
}