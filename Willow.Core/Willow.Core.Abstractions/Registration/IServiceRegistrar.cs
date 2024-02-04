using Microsoft.Extensions.DependencyInjection;

namespace Willow.Registration;

//GUIDE_REQUIRED SERVICE REGISTRATION
/// <summary>
/// Registration point for modules to register with the DI.
/// </summary>
/// <remarks>
/// Make sure all the services registered here are singletons. <br/>
/// Loaded via <see cref="IAssemblyRegistrar"/> <br/>
/// <b><i>IMPORTANT!</i></b> in a divergence from microsoft and most DI containers, when registering an interface to a concrete
/// type <c>AddSingleton{TInterface, TConcrete}()</c> - the interface gets registered as a mapping to the concrete type!
/// This is essentially because the plugin architecture means services might inherit many interfaces but only one
/// implementation is required. <br/>
/// <b><i>IMPORTANT!</i></b> Make sure not to register any duplicates or types that are already loaded via reflection,
/// there is not deduplication happening, so if attempting to register a type defined in another system check if it is
/// implemented by a <see cref="IAssemblyRegistrar"/> to read it. Types should also contain this information of their docs.
/// </remarks>
public interface IServiceRegistrar
{
    /// <inheritdoc cref="IServiceRegistrar" />
    void RegisterServices(IServiceCollection services);
}
