using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers;
using Willow.Core.Logging.Settings;

namespace Willow.Core.Logging;

internal class LoggingRegistration : IConfigurationRegistrar
{
    public static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PrivacySettings>(configuration.GetSection("privacy"));
    }
}