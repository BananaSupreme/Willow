using System.Reflection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;

namespace Willow.Core.SpeechCommands.VoiceCommandCompilation.Registration;

internal class VoiceCommandCompilationAssemblyRegistrar : IAssemblyRegistrar
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