using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Eventing.Events;

public readonly record struct CommandModifiedEvent(RawVoiceCommand[] Commands);
