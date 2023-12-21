using System.Reflection;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

public interface IEventRegistrar
{
    void RegisterEventsFromAssemblies(Assembly[] assemblies);
}