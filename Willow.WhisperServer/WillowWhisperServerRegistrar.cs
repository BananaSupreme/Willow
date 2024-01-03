using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Willow.Core.Registration.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Speech.SpeechRecognition.SpeechToText.Abstractions;

namespace Willow.WhisperServer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WillowWhisperServerRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddHostedService<WhisperEngine>();
        services.AddSingleton(provider => (ISpeechToTextEngine)provider.GetRegisteredAs<WhisperEngine, IHostedService>());
    }
}