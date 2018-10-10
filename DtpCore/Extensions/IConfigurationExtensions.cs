using Microsoft.Extensions.Configuration;

namespace DtpCore.Extensions
{
    public static class IConfigurationExtensions
    {

        public static int WorkflowInterval(this IConfiguration configuration, int defaultValue = 60*10) // 10 minutes
        {
            return configuration.GetValue("workflowinterval", defaultValue); 
        }

        public static int TrustPackageWorkflowInterval(this IConfiguration configuration, int defaultValue = 60 * 60 * 24) // 24 hours
        {
            return configuration.GetValue("TrustPackageWorkflowInterval", defaultValue); // 10 minutes
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



    }
}
