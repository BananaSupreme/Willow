using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Speech.SpeechRecognition.SpeechToText.Abstractions;

namespace Willow.Vosk.Registration;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class VoskServerRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddHostedService<VoskEngine>();
        services.AddSingleton(provider => (ISpeechToTextEngine)provider.GetRegisteredAs<VoskEngine, IHostedService>());
    }
}