using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.VAD.Abstractions;

namespace Willow.Speech.SpeechRecognition.VAD.Registration;

internal sealed class VadRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IVoiceActivityDetection, SileroVoiceActivityDetectionFacade>();
    }
}