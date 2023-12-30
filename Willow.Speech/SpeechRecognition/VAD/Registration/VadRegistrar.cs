using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.VAD.Abstractions;
using Willow.Speech.SpeechRecognition.VAD.Settings;

namespace Willow.Speech.SpeechRecognition.VAD.Registration;

internal sealed class VadRegistrar : IServiceRegistrar, IConfigurationRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IVoiceActivityDetection, SileroVoiceActivityDetectionFacade>();
    }

    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SileroSettings>(configuration.GetSection("Silero"));
    }
}