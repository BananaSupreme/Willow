using System.Reflection;

using Willow.Registration;

namespace Willow.Speech.Tokenization.Registration;

/// <summary>
/// Registers all the <see cref="ITranscriptionTokenizer" /> in the assemblies.
/// </summary>
internal sealed class TokenizationAssemblyRegistrar : IAssemblyRegistrar
{
    public Type[] ExtensionTypes => [typeof(ITranscriptionTokenizer)];

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
}
