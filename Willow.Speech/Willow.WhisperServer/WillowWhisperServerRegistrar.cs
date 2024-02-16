using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.SpeechToText.Extensions;

namespace Willow.WhisperServer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WillowWhisperServerRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSpeechToTextEngine<WhisperEngine>();
    }
}
