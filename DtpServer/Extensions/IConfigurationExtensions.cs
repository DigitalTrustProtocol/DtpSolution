using Microsoft.Extensions.Configuration;

namespace DtpServer.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string RateLimits(this IConfiguration configuration, string defaultValue = "Off")
        {
            return configuration.GetValue("ratelimits", defaultValue); // zone=ALL rate=10r/s burst=10 nodelay
        }
    }
}
