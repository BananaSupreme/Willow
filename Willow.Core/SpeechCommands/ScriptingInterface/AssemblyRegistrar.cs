using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Helpers.Logging;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Events;
using Willow.Core.SpeechCommands.Tokenization.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandParsing.Abstractions;

namespace Willow.Core.SpeechCommands.ScriptingInterface;

internal class AssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IEventRegistrar _eventRegistrar;
    private readonly IInterfaceRegistrar _interfaceRegistrar;
    private readonly IVoiceCommandInterpreter _commandInterpreter;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AssemblyRegistrar> _log;

    private static readonly Type[] _typesToRegister = [typeof(IVoiceCommand), typeof(INodeProcessor), typeof(INodeCompiler), typeof(ISpecializedTokenProcessor)];
    
    public AssemblyRegistrar(IEventRegistrar eventRegistrar, 
                             IInterfaceRegistrar interfaceRegistrar,
                             IVoiceCommandInterpreter commandInterpreter, 
                             IEventDispatcher eventDispatcher,
                             IServiceProvider serviceProvider, 
                             ILogger<AssemblyRegistrar> log)
    {
        _eventRegistrar = eventRegistrar;
        _interfaceRegistrar = interfaceRegistrar;
        _commandInterpreter = commandInterpreter;
        _eventDispatcher = eventDispatcher;
        _serviceProvider = serviceProvider;
        _log = log;
    }

    public void RegisterAssemblies(Assembly[] assemblies)
    {
        _log.ProcessingAssemblies(new(assemblies.Select(x => x.GetName().FullName)));
        _eventRegistrar.RegisterEventsFromAssemblies(assemblies);
        foreach (var type in _typesToRegister)
        {
            _interfaceRegistrar.RegisterDeriving(type, assemblies);
        }

        DispatchCommands();
    }

    private void DispatchCommands()
    {
        var commands = _serviceProvider.GetServices<IVoiceCommand>();
        var rawVoiceCommands = commands.Select(_commandInterpreter.InterpretCommand).ToArray();
        _eventDispatcher.Dispatch(new CommandModifiedEvent(rawVoiceCommands));
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Processing assemblies: {assemblyNames}")]
    public static partial void ProcessingAssemblies(this ILogger logger, LoggingEnumerator<string> assemblyNames);
}