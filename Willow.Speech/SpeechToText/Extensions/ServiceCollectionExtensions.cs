using Microsoft.Extensions.DependencyInjection;

using Willow.Speech.SpeechToText.Abstractions;

namespace Willow.Speech.SpeechToText.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpeechToTextEngine<T>(this IServiceCollection services)
        where T : class, ISpeechToTextEngine
    {
        services.AddSingleton<T>();
        return services.AddSingleton<ISpeechToTextEngine>(provider => provider.GetRequiredService<T>());
    }
}