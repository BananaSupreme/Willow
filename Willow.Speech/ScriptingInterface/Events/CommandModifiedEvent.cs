using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Events;

public readonly record struct CommandModifiedEvent(RawVoiceCommand[] Commands);
