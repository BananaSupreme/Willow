using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.Microphone.Abstractions;

namespace Willow.Speech.Microphone.Registration;

internal sealed class MicrophoneRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMicrophoneAccess, MicrophoneAccess>();
        services.AddHostedService<MicrophoneWorker>();
    }
}