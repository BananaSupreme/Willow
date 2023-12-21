using Willow.Core.Environment.Models;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Models;

public readonly record struct RawVoiceCommand(
    Guid Id,
    string[] InvocationPhrases,
    TagRequirement[] TagRequirements,
    Dictionary<string, object> CapturedValues,
    string Description
);