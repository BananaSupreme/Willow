namespace Willow.Speech.ScriptingInterface.Eventing.Events;

/// <summary>
/// The commands in the system were modified or their processing were modified in some way that requires reconstruction.
/// </summary>
public readonly record struct CommandReconstructionRequestedEvent;
