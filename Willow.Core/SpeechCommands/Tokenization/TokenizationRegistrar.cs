using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers;
using Willow.Core.Helpers.Extensions;
using Willow.Core.SpeechCommands.Tokenization.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization;

internal class TokenizationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ITokenizer, Tokenizer>();
        services.AddAllTypesFromOwnAssembly<ISpecializedTokenProcessor>(ServiceLifetime.Singleton);
    }
}