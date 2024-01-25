using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Registration;

internal sealed class CollectionProvider<T> : ICollectionProvider<T>
{
    private readonly IServiceProvider _serviceProvider;

    public CollectionProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<T> Get()
    {
        return _serviceProvider.GetServices<T>();
    }
}
