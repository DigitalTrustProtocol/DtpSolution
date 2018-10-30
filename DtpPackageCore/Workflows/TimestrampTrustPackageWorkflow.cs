using DtpCore.Workflows;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace DtpPackageCore.Workflows
{
    /// <summary>
    /// Makes sure to timestamp a package
    /// </summary>
    [DisplayName("Timestramp Trust Packages")]
    [Description("Timestamp trust packages and update the package with timestamp information.")]
    public class TimestrampTrustPackageWorkflow : WorkflowContext
    {

        private ILogger<TimestrampTrustPackageWorkflow> _logger;

        public TimestrampTrustPackageWorkflow(ILogger<TimestrampTrustPackageWorkflow> logger)
        {
            _logger = logger;
        }

        public override void Execute()
        {
        }
    }
}
