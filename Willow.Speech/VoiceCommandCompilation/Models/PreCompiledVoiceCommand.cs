using Willow.Core.Environment.Models;

namespace Willow.Speech.VoiceCommandCompilation.Models;

public readonly record struct PreCompiledVoiceCommand(
    Guid Id,
    string InvocationPhrase,
    TagRequirement[] TagRequirements,
    Dictionary<string, object> CapturedValues);