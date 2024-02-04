using Microsoft.Extensions.DependencyInjection;

namespace Willow.Registration;

internal sealed partial class AssemblyRegistrationEntry
{
    private static DescriptorRecordPair GetServiceRegistrationAndCorrectDescriptor(ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.ImplementationType is not null)
        {
            return DescriptorRecordPairFromImplementationType(serviceDescriptor);
        }

        if (serviceDescriptor.ImplementationInstance is not null)
        {
            return DescriptorRecordPairFromInstance(serviceDescriptor);
        }

        if (serviceDescriptor.ImplementationFactory is not null)
        {
            return DescriptorRecordPairFromFactory(serviceDescriptor);
        }

        return new DescriptorRecordPair(serviceDescriptor,
                                        new ServiceRegistrationRecord(serviceDescriptor.ServiceType,
                                                                      serviceDescriptor.ServiceType));
    }

    private static DescriptorRecordPair DescriptorRecordPairFromImplementationType(ServiceDescriptor serviceDescriptor)
    {
        return new DescriptorRecordPair(
            new ServiceDescriptor(serviceDescriptor.ImplementationType!,
                                  serviceDescriptor.ImplementationType!,
                                  serviceDescriptor.Lifetime),
            new ServiceRegistrationRecord(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType!));
    }

    private static DescriptorRecordPair DescriptorRecordPairFromInstance(ServiceDescriptor serviceDescriptor)
    {
        return new DescriptorRecordPair(
            new ServiceDescriptor(serviceDescriptor.ImplementationInstance!.GetType(),
                                  serviceDescriptor.ImplementationInstance),
            new ServiceRegistrationRecord(serviceDescriptor.ServiceType, serviceDescriptor.GetImplementationType()));
    }

    private static DescriptorRecordPair DescriptorRecordPairFromFactory(ServiceDescriptor serviceDescriptor)
    {
        var typeArguments = serviceDescriptor.ImplementationFactory!.GetType().GenericTypeArguments;
        var implementationType = typeArguments[1];

        return new DescriptorRecordPair(
            new ServiceDescriptor(implementationType,
                                  serviceDescriptor.ImplementationFactory,
                                  serviceDescriptor.Lifetime),
            new ServiceRegistrationRecord(serviceDescriptor.ServiceType, serviceDescriptor.GetImplementationType()));
    }
}

file static class ServiceDescriptorExtensions
{
    public static Type GetImplementationType(this ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.ImplementationType != null)
        {
            return serviceDescriptor.ImplementationType;
        }

        if (serviceDescriptor.ImplementationInstance != null)
        {
            return serviceDescriptor.ImplementationInstance.GetType();
        }

        if (serviceDescriptor.ImplementationFactory != null)
        {
            var typeArguments = serviceDescriptor.ImplementationFactory.GetType().GenericTypeArguments;
            return typeArguments[1];
        }

        return serviceDescriptor.ServiceType;
    }
}
