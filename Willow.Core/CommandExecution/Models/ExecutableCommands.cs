using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

namespace Willow.Core.CommandExecution.Models;

public readonly record struct ExecutableCommands(Guid Id, Func<IVoiceCommand> CommandActivator);