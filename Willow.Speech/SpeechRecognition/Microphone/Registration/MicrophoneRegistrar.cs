using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.Microphone.Abstractions;

namespace Willow.Speech.SpeechRecognition.Microphone.Registration;

internal sealed class MicrophoneRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMicrophoneAccess, MicrophoneAccess>();
        services.AddHostedService<MicrophoneWorker>();
    }
}