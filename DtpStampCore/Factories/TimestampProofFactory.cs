using System.Collections.Generic;
using System.Linq;
using DtpCore.Model;
using DtpCore.Services;
using DtpStampCore.Interfaces;

namespace DtpStampCore.Factories
{
    public class BlockchainProofFactory : IBlockchainProofFactory
    {
        //private ITrustDBService _trustDBService;
        private IWorkflowService _workflowService;

        public BlockchainProofFactory(IWorkflowService workflowService)
        {
            //_trustDBService = trustDBService;
            _workflowService = workflowService;
        }

        public BlockchainProof Create(Timestamp timestamp)
        {
            var workflowContainer = _workflowService.Workflows.FirstOrDefault(p => p.DatabaseID == timestamp.WorkflowID);
            if (workflowContainer == null)
                return null;

            var workflow = (ITimestampWorkflow)_workflowService.Create(workflowContainer);

            var proof = workflow.Proof;
            proof.Proofs = new List<Timestamp>
            {
                timestamp
            };

            return proof;
        }
    }
}