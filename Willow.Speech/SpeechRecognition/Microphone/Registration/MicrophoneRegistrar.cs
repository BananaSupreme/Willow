using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.Microphone.Abstractions;
using Willow.Speech.SpeechRecognition.Microphone.Settings;

namespace Willow.Speech.SpeechRecognition.Microphone.Registration;

internal sealed class MicrophoneRegistrar : IServiceRegistrar, IConfigurationRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMicrophoneAccess, MicrophoneAccess>();
        services.AddHostedService<MicrophoneWorker>();
    }

    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MicrophoneSettings>(configuration.GetSection("Microphone"));
    }
}