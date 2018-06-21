using Microsoft.Extensions.Configuration;

namespace DtpCore.Extensions
{
    public static class IConfigurationExtensions
    {

        public static int WorkflowInterval(this IConfiguration configuration, int defaultValue = 60*10) // 10 minutes
        {
            return configuration.GetValue("workflowinterval", defaultValue); 
        }
    }
}
