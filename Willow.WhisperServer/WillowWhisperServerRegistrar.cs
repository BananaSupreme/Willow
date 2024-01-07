using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechToText.Extensions;

namespace Willow.WhisperServer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WillowWhisperServerRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSpeechToTextEngine<WhisperEngine>();
    }
}