using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.AudioBuffering.Abstractions;

namespace Willow.Speech.SpeechRecognition.AudioBuffering.Registration;

internal sealed class AudioBufferingRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAudioBuffer, AudioBuffer>();
    }
}