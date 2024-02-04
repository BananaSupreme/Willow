using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.VoiceCommandCompilation.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

internal sealed class VoiceCommandCompilationRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ITrieFactory, TrieFactory>();
        services.AddSingleton<IVoiceCommandCompiler, VoiceCommandCompiler>();
    }
}
