using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

/// <summary>
/// Registers all the <see cref="INodeCompiler" /> in the assemblies.
/// </summary>
internal sealed class VoiceCommandCompilationAssemblyRegistrar : IAssemblyRegistrar
{
    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        services.AddAllTypesDeriving<INodeCompiler>(assembly);
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
}
