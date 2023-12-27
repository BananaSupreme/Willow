using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers.Extensions;
using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.Tokenization.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Registration;

internal class TokenizationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrar, TokenizationAssemblyRegistrar>();
        services.AddSingleton<ITokenizer, Tokenizer>();
        services.AddAllTypesFromOwnAssembly<ISpecializedTokenProcessor>(ServiceLifetime.Singleton);
    }
}