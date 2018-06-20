using Microsoft.Extensions.Logging;
using TrustchainCore.Enumerations;
using TrustchainCore.Interfaces;

namespace TrustchainCore.Workflows
{
    public class SuccessStep : WorkflowStep, ISuccessStep
    {
        private ILogger<SuccessStep> _logger;

        public SuccessStep(ILogger<SuccessStep> logger)
        {
            _logger = logger;

        }
        public override void Execute()
        {
            CombineLog(_logger,$"Workflow executed successfully");
            Context.Container.State = WorkflowStatusType.Finished.ToString();
        }
    }
}
