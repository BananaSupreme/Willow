using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechRecognition.Resampling.Abstractions;

namespace Willow.Speech.SpeechRecognition.Resampling.Registration;

internal sealed class ResamplingRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IResampler, Resampler>();
    }
}