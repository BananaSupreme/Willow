using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.AudioBuffering.Abstractions;
using Willow.Speech.SpeechRecognition.AudioBuffering.Settings;

namespace Willow.Speech.SpeechRecognition.AudioBuffering.Registration;

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