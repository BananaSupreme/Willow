using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Helpers.Extensions;
using Willow.Registration;

namespace Willow.Speech.Tokenization.Registration;

/// <summary>
/// Registers all the <see cref="ITranscriptionTokenizer" /> in the assemblies.
/// </summary>
internal sealed class TokenizationAssemblyRegistrar : IAssemblyRegistrar
{
    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        services.AddAllTypesDeriving<ITranscriptionTokenizer>(assembly);
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
