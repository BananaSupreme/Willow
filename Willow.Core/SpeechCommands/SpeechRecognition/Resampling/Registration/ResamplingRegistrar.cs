using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.SpeechRecognition.Resampling.Abstractions;

namespace Willow.Core.SpeechCommands.SpeechRecognition.Resampling.Registration;

internal sealed class ResamplingRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IResampler, Resampler>();
    }
}