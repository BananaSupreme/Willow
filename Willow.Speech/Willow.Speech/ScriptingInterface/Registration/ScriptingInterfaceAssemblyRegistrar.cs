using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Eventing;
using Willow.Helpers.Extensions;
using Willow.Registration;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Eventing.Events;

namespace Willow.Speech.ScriptingInterface.Registration;

/// <summary>
/// Adds all <see cref="IVoiceCommand"/> in the assemblies and dispatches an updated to the commands.
/// </summary>
internal sealed class ScriptingInterfaceAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IVoiceCommandInterpreter _voiceCommandInterpreter;

    public ScriptingInterfaceAssemblyRegistrar(IVoiceCommandInterpreter voiceCommandInterpreter,
                                               IEventDispatcher eventDispatcher)
    {
        _voiceCommandInterpreter = voiceCommandInterpreter;
        _eventDispatcher = eventDispatcher;
    }

    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        services.AddAllTypesDeriving<IVoiceCommand>(assembly);
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        var commands = serviceProvider.GetServices<IVoiceCommand>().ToArray();
        if (commands.Length == 0)
        {
            return Task.CompletedTask;
        }

        var actualized = commands.Select(_voiceCommandInterpreter.InterpretCommand);
        _eventDispatcher.Dispatch(new CommandsAddedEvent(actualized.ToArray()));
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        var commands = serviceProvider.GetServices<IVoiceCommand>().ToArray();
        if (commands.Length == 0)
        {
            return Task.CompletedTask;
        }

        var actualized = commands.Select(_voiceCommandInterpreter.InterpretCommand);
        _eventDispatcher.Dispatch(new CommandsRemovedEvent(actualized.ToArray()));
        return Task.CompletedTask;
    }
}
