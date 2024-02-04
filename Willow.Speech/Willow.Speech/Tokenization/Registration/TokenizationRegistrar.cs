using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.Tokenization.Abstractions;

namespace Willow.Speech.Tokenization.Registration;

internal sealed class TokenizationRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ITokenizer, Tokenizer>();
        services.AddSingleton<IHomophonesDictionaryLoader, HomophonesDictionaryLoader>();
    }
}
