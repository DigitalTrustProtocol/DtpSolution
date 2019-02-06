using DtpCore.Extensions;
using Microsoft.Extensions.Configuration;

namespace DtpStampCore.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string FundingKey(this IConfiguration configuration, string blockchain = null, string defaultValue = "")
        {
            if (blockchain == null)
                blockchain = configuration.Blockchain();
            return configuration.GetValue(blockchain + "_fundingkey", defaultValue);
        }

        public static int StepRetryAttemptWait(this IConfiguration configuration, int defaultValue = 60 * 10)
        {
            return configuration.GetValue("stepretryattemptwait", defaultValue); // 10 minutes
        }

        public static int TimestampInterval(this IConfiguration configuration, int defaultValue = 1) // Change to 1
        {
            return configuration.GetValue("timestampinterval", defaultValue); // 10 minutes
        }

        public static int ConfirmationThreshold(this IConfiguration configuration, string blockchain = null, int defaultValue = 1) // Normal 6, 
        {
            if (blockchain == null)
                blockchain = configuration.Blockchain();
            return configuration.GetValue(blockchain + "_confirmationthreshold", configuration.GetValue("confirmationthreshold", defaultValue)); // 1 block confimations
        }
        public static int ConfirmationWait(this IConfiguration configuration, string blockchain = null, int defaultValue = 60*10)
        {
            if (blockchain == null)
                blockchain = configuration.Blockchain();
            return configuration.GetValue(blockchain + "_confirmationwait", configuration.GetValue("confirmationwait", defaultValue)); // 10 minutes
        }
    }
}
