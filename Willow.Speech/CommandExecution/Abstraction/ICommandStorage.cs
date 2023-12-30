using Willow.Speech.CommandExecution.Models;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.CommandExecution.Abstraction;

public interface ICommandStorage
{
    internal void SetAvailableCommands(ExecutableCommands[] commands);
    Task ExecuteCommandAsync(Guid id, VoiceCommandContext context);
}