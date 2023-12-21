using Willow.Core.CommandExecution.Models;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;

namespace Willow.Core.CommandExecution.Abstraction;

public interface ICommandStorage
{
    internal void SetAvailableCommands(ExecutableCommands[] commands);
    Task ExecuteCommandAsync(Guid id, VoiceCommandContext context);
}