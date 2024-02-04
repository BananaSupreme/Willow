using System.Collections;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Willow.Registration;

internal sealed partial class AssemblyRegistrationEntry
{
    private sealed class AssemblyBoundServiceProviderWrapper : IServiceProvider
    {
        private readonly IServiceProvider _inner;
        private readonly Assembly _assembly;

        public AssemblyBoundServiceProviderWrapper(IServiceProvider inner, Assembly assembly)
        {
            _inner = inner;
            _assembly = assembly;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var inner = _inner.GetServices(serviceType.GetGenericArguments()[0])
                                  .Where(static x => x is not null)
                                  .Where(x => _assembly.DefinedTypes.Contains(x!.GetType()));
                var activate = typeof(EnumerableWrapper<>).MakeGenericType(serviceType.GetGenericArguments()[0]);
                return Activator.CreateInstance(activate, inner);
            }

            return _assembly.DefinedTypes.Contains(serviceType) ? _inner.GetService(serviceType) : null;
        }

        //Basically there was no other way to Hard Type this, other than this weird wrapper, but this works, so there it is
        private sealed class EnumerableWrapper<T> : IEnumerable<T>
        {
            private readonly IEnumerable<object> _input;

            public EnumerableWrapper(IEnumerable<object> input)
            {
                _input = input;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _input.Cast<T>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _input.GetEnumerator();
            }
        }
    }
}
