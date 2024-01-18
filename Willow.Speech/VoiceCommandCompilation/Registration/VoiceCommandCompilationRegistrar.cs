using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

internal sealed class VoiceCommandCompilationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrar, VoiceCommandCompilationAssemblyRegistrar>();
        services.AddSingleton<ITrieFactory, TrieFactory>();
        services.AddSingleton<IVoiceCommandCompiler, VoiceCommandCompiler>();
    }
}
