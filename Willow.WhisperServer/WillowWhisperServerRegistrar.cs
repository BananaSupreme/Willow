using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Willow.Core.Helpers.Extensions;
using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.SpeechToText.Abstractions;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WillowWhisperServerRegistrar : IServiceRegistrar, IConfigurationRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddHostedService<WhisperEngine>();
        services.AddSingleton(provider => (ISpeechToTextEngine)provider.GetRegisteredAs<WhisperEngine, IHostedService>());
    }

    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WhisperModelSettings>(configuration.GetSection("WhisperModel"));
        services.Configure<TranscriptionSettings>(configuration.GetSection("Transcription"));
    }
}