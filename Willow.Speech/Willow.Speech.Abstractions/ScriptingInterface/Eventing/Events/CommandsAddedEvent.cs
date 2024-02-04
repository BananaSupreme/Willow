using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Eventing.Events;

/// <summary>
/// New commands were added into the system.
/// </summary>
/// <param name="Commands">The commands added.</param>
public readonly record struct CommandsAddedEvent(RawVoiceCommand[] Commands);