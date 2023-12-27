using System.Reflection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.Tokenization.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Registration;

internal class TokenizationAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IInterfaceRegistrar _interfaceRegistrar;

    public TokenizationAssemblyRegistrar(IInterfaceRegistrar interfaceRegistrar)
    {
        _interfaceRegistrar = interfaceRegistrar;
    }
    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        _interfaceRegistrar.RegisterDeriving<ISpecializedTokenProcessor>(assemblies);
    }
}