using System.Reflection;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

// ReSharper disable once UnusedTypeParameter
public interface IInterfaceRegistrar
{
    void RegisterDeriving(Type typeToDeriveFrom, Assembly[] assemblies);
    void RegisterDeriving<T>(Assembly[] assemblies);
}