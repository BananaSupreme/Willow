using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Privacy.Settings;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Privacy.Registration;

internal sealed class PrivacyRegistrar : IConfigurationRegistrar
{
    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PrivacySettings>(configuration.GetSection("privacy"));
    }
}