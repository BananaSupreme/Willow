using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface.Eventing.Events;

/// <summary>
/// The commands in the system were modified and a new collection of commands should be considered.
/// </summary>
/// <param name="Commands">The new commands in the system.</param>
public readonly record struct CommandModifiedEvent(RawVoiceCommand[] Commands);
