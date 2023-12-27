using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Registration;

internal class VoiceCommandCompilationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrar, VoiceCommandCompilationAssemblyRegistrar>();
        services.AddSingleton<ITrieFactory, TrieFactory>();
        services.AddSingleton<IVoiceCommandCompiler, VoiceCommandCompiler>();
    }
}