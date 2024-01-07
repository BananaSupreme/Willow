using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;

namespace Willow.Speech.ScriptingInterface.Models;

public readonly record struct RawVoiceCommand(
    Guid Id,
    string[] InvocationPhrases,
    TagRequirement[] TagRequirements,
    Dictionary<string, object> CapturedValues,
    SupportedOperatingSystems SupportedOperatingSystems,
    string Name,
    string Description
);