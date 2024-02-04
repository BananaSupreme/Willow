using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Eventing;
using Willow.Helpers.Extensions;
using Willow.Registration;
using Willow.Speech.ScriptingInterface.Eventing.Events;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

/// <summary>
/// Registers all the <see cref="INodeCompiler" /> in the assemblies.
/// </summary>
internal sealed class VoiceCommandCompilationAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IEventDispatcher _eventDispatcher;

    public VoiceCommandCompilationAssemblyRegistrar(IEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        services.AddAllTypesDeriving<INodeCompiler>(assembly);
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
