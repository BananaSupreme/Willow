using System.Reflection;

namespace Willow.Core.Registration.Abstractions;

// ReSharper disable once UnusedTypeParameter
public interface IInterfaceRegistrar
{
    void RegisterDeriving(Type typeToDeriveFrom, Assembly[] assemblies);
    void RegisterDeriving<T>(Assembly[] assemblies);
}