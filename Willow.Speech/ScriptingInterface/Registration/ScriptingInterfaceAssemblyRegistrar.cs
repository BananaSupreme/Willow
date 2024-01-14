using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Registration.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Eventing.Events;

namespace Willow.Speech.ScriptingInterface.Registration;

/// <summary>
/// Adds all voice commands in the assemblies and dispatches an updated to the commands.
/// </summary>
internal sealed class ScriptingInterfaceAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVoiceCommandInterpreter _voiceCommandInterpreter;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IInterfaceRegistrar _interfaceRegistrar;

    public ScriptingInterfaceAssemblyRegistrar(IServiceProvider serviceProvider,
                                               IVoiceCommandInterpreter voiceCommandInterpreter,
                                               IEventDispatcher eventDispatcher,
                                               IInterfaceRegistrar interfaceRegistrar)
    {
        _serviceProvider = serviceProvider;
        _voiceCommandInterpreter = voiceCommandInterpreter;
        _eventDispatcher = eventDispatcher;
        _interfaceRegistrar = interfaceRegistrar;
    }

    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        _interfaceRegistrar.RegisterDeriving<IVoiceCommand>(assemblies);
        DispatchCommands();
    }

    private void DispatchCommands()
    {
        var commands = _serviceProvider.GetServices<IVoiceCommand>();
        var rawVoiceCommands = commands.Select(_voiceCommandInterpreter.InterpretCommand).ToArray();
        _eventDispatcher.Dispatch(new CommandModifiedEvent(rawVoiceCommands));
    }
}