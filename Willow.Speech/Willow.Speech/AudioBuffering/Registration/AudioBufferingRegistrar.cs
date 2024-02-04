using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.AudioBuffering.Abstractions;

namespace Willow.Speech.AudioBuffering.Registration;

internal sealed class AudioBufferingRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAudioBuffer, AudioBuffer>();
    }
}
