using Microsoft.Extensions.DependencyInjection;

namespace Willow.Core.Helpers.Extensions;

internal static class ServiceProviderExtensions
{
    public static T GetRegisteredAs<T, TInterface>(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetServices<TInterface>().OfType<T>().First();
    }
}