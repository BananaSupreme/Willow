using Willow.Core.Environment.Models;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Models;

public readonly record struct PreCompiledVoiceCommand(
    Guid Id,
    string InvocationPhrase,
    TagRequirement[] TagRequirements,
    Dictionary<string, object> CapturedValues);