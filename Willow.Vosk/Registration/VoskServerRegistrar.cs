using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechToText.Extensions;
using Willow.Vosk.Abstractions;

namespace Willow.Vosk.Registration;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class VoskServerRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSpeechToTextEngine<VoskEngine>();
        services.AddSingleton<IVoskModelInstaller, VoskModelInstaller>();
        services.AddHttpClient<IVoskModelDownloader, VoskModelDownloader>();
    }
}
