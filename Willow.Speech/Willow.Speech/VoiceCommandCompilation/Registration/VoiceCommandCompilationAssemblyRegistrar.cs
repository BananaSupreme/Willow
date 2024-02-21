using System.Reflection;

using Willow.Eventing;
using Willow.Registration;
using Willow.Speech.ScriptingInterface.Eventing.Events;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

/// <summary>
/// Registers all the <see cref="INodeCompiler" /> in the assemblies.
/// </summary>
internal sealed class VoiceCommandCompilationAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IEventDispatcher _eventDispatcher;
    public Type[] ExtensionTypes => [typeof(INodeCompiler)];

    public VoiceCommandCompilationAssemblyRegistrar(IEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        _eventDispatcher.Dispatch(new CommandReconstructionRequestedEvent());
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        _eventDispatcher.Dispatch(new CommandReconstructionRequestedEvent());
        return Task.CompletedTask;
    }
}
