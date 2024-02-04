using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.VAD.Abstractions;

namespace Willow.Speech.VAD.Registration;

internal sealed class VadRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IVoiceActivityDetection, SileroVoiceActivityDetectionFacade>();
    }
}
