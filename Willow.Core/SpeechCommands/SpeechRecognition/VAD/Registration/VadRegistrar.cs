using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.VAD.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.VAD.Settings;

namespace Willow.Core.SpeechCommands.SpeechRecognition.VAD.Registration;

internal class VadRegistrar : IServiceRegistrar, IConfigurationRegistrar
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