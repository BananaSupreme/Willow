using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers.Extensions;
using Willow.Core.Registration.Abstractions;
using Willow.Speech.Tokenization.Abstractions;

namespace Willow.Speech.Tokenization.Registration;

internal sealed class TokenizationRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrar, TokenizationAssemblyRegistrar>();
        services.AddSingleton<ITokenizer, Tokenizer>();
        services.AddAllTypesFromOwnAssembly<ISpecializedTokenProcessor>(ServiceLifetime.Singleton);
    }
}