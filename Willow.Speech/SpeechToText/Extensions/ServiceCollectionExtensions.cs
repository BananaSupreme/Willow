using Microsoft.Extensions.DependencyInjection;

using Willow.Speech.SpeechToText.Abstractions;

namespace Willow.Speech.SpeechToText.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a new speech to text engine with the system, it is available both as itself and under the
    /// <see cref="ISpeechToTextEngine"/> umbrella.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <typeparam name="T">Concrete type of speech to text engine.</typeparam>
    public static IServiceCollection AddSpeechToTextEngine<T>(this IServiceCollection services)
        where T : class, ISpeechToTextEngine
    {
        services.AddSingleton<T>();
        return services.AddSingleton<ISpeechToTextEngine>(provider => provider.GetRequiredService<T>());
    }
}