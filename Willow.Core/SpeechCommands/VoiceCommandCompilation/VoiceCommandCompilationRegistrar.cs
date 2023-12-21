using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers;
using Willow.Core.Helpers.Extensions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation;

internal class VoiceCommandCompilationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddAllTypesFromOwnAssembly<INodeCompiler>(ServiceLifetime.Singleton);
        services.AddSingleton<ITrieFactory, TrieFactory>();
        services.AddSingleton<IVoiceCommandCompiler, VoiceCommandCompiler>();
    }
}