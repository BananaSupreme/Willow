using System.Reflection;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

public interface IAssemblyRegistrar
{
    void RegisterAssemblies(Assembly[] assemblies);
}