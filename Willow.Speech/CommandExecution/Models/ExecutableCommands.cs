using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.CommandExecution.Models;

internal readonly record struct ExecutableCommands(Guid Id, Func<IVoiceCommand> CommandActivator);