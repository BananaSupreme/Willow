using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Eventing.Events;

/// <summary>
/// New commands were removed from the system.
/// </summary>
/// <param name="Commands">The commands removed.</param>
public readonly record struct CommandsRemovedEvent(RawVoiceCommand[] Commands);