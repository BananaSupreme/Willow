using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.Resampling.Abstractions;

namespace Willow.Speech.Resampling.Registration;

internal sealed class ResamplingRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IResampler, Resampler>();
    }
}
