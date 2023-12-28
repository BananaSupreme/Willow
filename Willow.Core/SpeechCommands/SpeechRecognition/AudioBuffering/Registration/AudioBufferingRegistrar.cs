using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.AudioBuffering.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.AudioBuffering.Settings;

namespace Willow.Core.SpeechCommands.SpeechRecognition.AudioBuffering.Registration;

internal sealed class AudioBufferingRegistrar : IServiceRegistrar, IConfigurationRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAudioBuffer, AudioBuffer>();
    }

    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AudioBufferSettings>(configuration.GetSection("AudioBuffer"));
    }
}