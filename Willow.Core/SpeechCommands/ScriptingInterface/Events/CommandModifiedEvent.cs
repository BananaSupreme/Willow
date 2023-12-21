using Willow.Core.SpeechCommands.ScriptingInterface.Models;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Events;

public readonly record struct CommandModifiedEvent(RawVoiceCommand[] Commands);
