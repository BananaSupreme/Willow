using System.Reflection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Registration;

/// <summary>
/// Registers all the <see cref="INodeCompiler" /> in the assemblies.
/// </summary>
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
