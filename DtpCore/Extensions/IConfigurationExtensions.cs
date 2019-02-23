using Microsoft.Extensions.Configuration;

namespace DtpCore.Extensions
{
    public static class IConfigurationExtensions
    {

        public static bool IsAdminEnabled(this IConfiguration configuration, bool defaultValue = false) 
        {
#if DEBUG
            return true;
#else
            return configuration.GetValue("AdminEnabled", defaultValue);
#endif


        }

        public static int WorkflowInterval(this IConfiguration configuration, int defaultValue = 60*10) // 10 minutes
        {
            return configuration.GetValue("workflowinterval", defaultValue); 
        }

        public static T GetModel<T>(this IConfiguration configuration, T model = default(T))
        {
            if (configuration == null)
                return model;

            var section = configuration.GetSection(nameof(T).Replace("Section", ""));
            if (section == null)
                return model;

            section.Bind(model);
            return model;
        }

        public static string ServerKeyword(this IConfiguration configuration, string defaultValue = "")
        {
            return configuration.GetValue("ServerKeyword", defaultValue);
        }


        public static string Blockchain(this IConfiguration configuration, string defaultValue = "btctest")
        {
            return configuration.GetValue("blockchain", defaultValue);
        }

    }
}
