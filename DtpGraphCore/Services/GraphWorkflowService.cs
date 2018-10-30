using DtpCore.Services;
using DtpGraphCore.Interfaces;

namespace DtpGraphCore.Services
{
    public class GraphWorkflowService : IGraphWorkflowService
    {
        private IWorkflowService _workflowService;

        public GraphWorkflowService(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }


        //public void EnsureTrustTimestampWorkflow()
        //{
        //    _workflowService.EnsureWorkflow<TrustTimestampWorkflow>();
        //}

    }
}
