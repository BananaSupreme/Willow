using System.Reflection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.Tokenization.Abstractions;

namespace Willow.Speech.Tokenization.Registration;

internal sealed class TokenizationAssemblyRegistrar : IAssemblyRegistrar
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