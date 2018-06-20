using Microsoft.Extensions.Configuration;
using TrustchainCore.Workflows;
using TruststampCore.Extensions;
using TruststampCore.Interfaces;

namespace TruststampCore.Workflows
{
    public class TimestampScheduleStep : WorkflowStep, ITimestampScheduleStep
    {
        private ITimestampWorkflowService _timestampWorkflowService;
        private IConfiguration _configuration;

        public TimestampScheduleStep(ITimestampWorkflowService timestampWorkflowService, IConfiguration configuration)
        {
            _timestampWorkflowService = timestampWorkflowService;
            _configuration = configuration;
        }

        public override void Execute()
        {
            if(_timestampWorkflowService.CountCurrentProofs() > 0)
            {
                _timestampWorkflowService.CreateAndExecute(); // There are proofs to be timestamp'ed
            }

            // Rerun this step after x time, never to exit
            Context.Wait(_configuration.TimestampInterval()); // Default 10 min
        }
    }
}
