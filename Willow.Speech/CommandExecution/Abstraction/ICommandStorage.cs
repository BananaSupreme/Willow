using Willow.Speech.CommandExecution.Eventing.Handlers;
using Willow.Speech.CommandExecution.Exceptions;
using Willow.Speech.CommandExecution.Models;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.CommandExecution.Abstraction;

/// <summary>
/// Stores the available commands for the system after they were processed in
/// <see cref="CommandModifiedEventHandler" />.
/// </summary>
internal interface ICommandStorage
{
    /// <summary>
    /// Adds to the available commands in the storage.
    /// </summary>
    /// <param name="commands">The commands to add.</param>
    internal void AddCommands(ExecutableCommands[] commands);

    /// <summary>
    /// Removes from the available commands in the storage.
    /// </summary>
    /// <param name="commands">The commands to remove.</param>
    internal void RemoveCommands(ExecutableCommands[] commands);

    /// <summary>
    /// Executes command.
    /// </summary>
    /// <param name="id">Id of command to execute.</param>
    /// <param name="context">The execution context of the command.</param>
    /// <exception cref="CommandNotFoundException">Calls when a command was requested but not found. HIGHLY UNUSUAL!</exception>
    Task ExecuteCommandAsync(Guid id, VoiceCommandContext context);
}
