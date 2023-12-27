using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Logging.Settings;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Logging.Registration;

internal class LoggingRegistration : IConfigurationRegistrar
{
    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PrivacySettings>(configuration.GetSection("privacy"));
    }
}