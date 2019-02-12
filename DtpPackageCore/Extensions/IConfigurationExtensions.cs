using Microsoft.Extensions.Configuration;

namespace DtpPackageCore.Extensions
{
    public static class IConfigurationExtensions
    {

        public static string PackageScope(this IConfiguration configuration, string defaultValue = "twitter.com") // 10 minutes
        {
            return configuration.GetValue("packageScope", defaultValue);
        }

        public static int TrustPackageWorkflowInterval(this IConfiguration configuration, int defaultValue = 60 * 60 * 24) // 24 hours
        {
            return configuration.GetValue("TrustPackageWorkflowInterval", defaultValue); // 10 minutes
        }

        public static int SynchronizePackageWorkflowInterval(this IConfiguration configuration, int defaultValue = 60 * 10) // 10 minutes
        {
            return configuration.GetValue("TrustPackageWorkflowInterval", defaultValue); // 10 minutes
        }
    }
}
