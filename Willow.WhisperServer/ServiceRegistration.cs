using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.SpeechRecognition.SpeechToText.Abstractions;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer;

public class ServiceRegistration : IServiceRegistrar, IConfigurationRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISpeechToTextEngine, WhisperEngine>();
    }

    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WhisperModelSettings>(configuration.GetSection("WhisperModel"));
        services.Configure<TranscriptionSettings>(configuration.GetSection("Transcription"));
        services.Configure<PythonSettings>(configuration.GetSection("Python"));    
    }
}