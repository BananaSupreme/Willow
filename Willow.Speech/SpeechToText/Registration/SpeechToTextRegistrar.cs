using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;

namespace Willow.Speech.SpeechToText.Registration;

internal sealed class SpeechToTextRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddHostedService<SpeechEngineSelectorWorker>();
    }
}