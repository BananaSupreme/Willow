using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.CommandExecution.Models;

public readonly record struct ExecutableCommands(Guid Id, Func<IVoiceCommand> CommandActivator);