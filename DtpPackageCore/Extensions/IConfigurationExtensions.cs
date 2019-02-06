using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpPackageCore.Extensions
{
    public static class IConfigurationExtensions
    {

        public static string PackageScope(this IConfiguration configuration, string defaultValue = "twitter.com") // 10 minutes
        {
            return configuration.GetValue("packageScope", defaultValue);
        }
    }
}
