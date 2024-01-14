﻿using Willow.Core.Environment.Enums;
using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines the relevant activation method for the <see cref="IVoiceCommand"/>
/// </summary>
public sealed class ActivationModeAttribute : Attribute
{
    /// <summary>
    /// Selected activation mode.
    /// </summary>
    public ActivationMode ActivationMode { get; }

    public ActivationModeAttribute(ActivationMode activationMode)
    {
        ActivationMode = activationMode;
    }
}