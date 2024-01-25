using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.SpeechToText.Extensions;
using Willow.Vosk.Abstractions;

namespace Willow.Vosk.Registration;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class VoskServerRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSpeechToTextEngine<VoskEngine>();
        services.AddSingleton<IVoskModelInstaller, VoskModelInstaller>();
        services.AddSingleton<IVoskModelDownloader, VoskModelDownloader>();
    }
}
