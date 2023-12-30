using System.Reflection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

internal sealed class VoiceCommandCompilationAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly IInterfaceRegistrar _interfaceRegistrar;

    public VoiceCommandCompilationAssemblyRegistrar(IInterfaceRegistrar interfaceRegistrar)
    {
        _interfaceRegistrar = interfaceRegistrar;
    }
    public void RegisterFromAssemblies(Assembly[] assemblies)
    {
        _interfaceRegistrar.RegisterDeriving<INodeCompiler>(assemblies);
    }
}