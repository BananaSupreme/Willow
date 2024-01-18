using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.CommandExecution.Models;

/// <summary>
/// A command ready for execution within the system.
/// </summary>
/// <param name="Id">The id of the command.</param>
/// <param name="CommandActivator">A function to activate to command.</param>
internal readonly record struct ExecutableCommands(Guid Id, Func<IVoiceCommand> CommandActivator);
