using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.Microphone.Abstractions;

namespace Willow.Speech.Microphone.Registration;

internal sealed class MicrophoneRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMicrophoneAccess, MicrophoneAccess>();
    }
}
