using System.Linq;
using Microsoft.Extensions.Configuration;
using TrustgraphCore.Interfaces;
using TrustchainCore.Workflows;
using TruststampCore.Extensions;
using TruststampCore.Interfaces;
using System.Collections.Generic;
using TrustchainCore.Services;
using TrustchainCore.Interfaces;
using Microsoft.Extensions.Logging;
using TrustchainCore.Model;

namespace TrustgraphCore.Workflows
{
    public class TrustTimestampStep : WorkflowStep, ITrustTimestampStep
    {
        private IWorkflowService _workflowService;
        private ITrustDBService _trustDBService;
        private ITimestampService _timestampService;
        private IConfiguration _configuration;
        private ITimestampSynchronizationService _timestampSynchronizationService;

        private ILogger<TrustTimestampStep> _logger;

        private Dictionary<int, ITimestampWorkflow> _workflows = new Dictionary<int, ITimestampWorkflow>();

        private int _addedProofs = 0;
        private int _updatedTrusts = 0;



        public TrustTimestampStep(IWorkflowService workflowService, ITrustDBService trustDBService, ITimestampService timestampService, IConfiguration configuration, ITimestampSynchronizationService timestampSynchronizationService, ILogger<TrustTimestampStep> logger)
        {
            _workflowService = workflowService;
            _trustDBService = trustDBService;
            _timestampService = timestampService;
            _configuration = configuration;
            _timestampSynchronizationService = timestampSynchronizationService;
            _logger = logger;

        }

        /// <summary>
        /// Looks up Trust that has not been updated with a timestamp.
        /// Ensures to update the trusts when their Proofs have been timestamped.
        /// </summary>
        public override void Execute()
        {
            // Get trusts
            var trusts = from t in _trustDBService.Trusts
                         where t.Timestamps == null 
                         select t;

            if (trusts != null && trusts.Count() > 0)
            {
                foreach (var trust in trusts)
                {
                    ProcessTrust(trust);
                }

                _trustDBService.DBContext.SaveChanges();

                if (_addedProofs > 0)
                    CombineLog(_logger, $"Added Proofs {_addedProofs}");

                if (_updatedTrusts > 0)
                    CombineLog(_logger, $"Updated trusts {_updatedTrusts}");
            }

            // Rerun this step after x time, never to exit
            Context.Wait(_configuration.TimestampInterval()); // Default 10 min
        }

        public void ProcessTrust(Trust trust)
        {
            var proof = _timestampService.Get(trust.Id);
            if (proof == null)
            {
                _timestampService.Add(trust.Id); // Fail safe, if somehow the trust has not been added to the proof list.
                _addedProofs++;
                return;
            }

            if (proof.WorkflowID == 0)
                return;

            // Use local caching of TimestampWorkflow to minimize load on DB
            if (!_workflows.TryGetValue(proof.WorkflowID, out ITimestampWorkflow timestampWorkflow))
            {
                timestampWorkflow = _workflowService.Load<ITimestampWorkflow>(proof.WorkflowID);
                if (timestampWorkflow == null)
                    return;

                _workflows.Add(proof.WorkflowID, timestampWorkflow);
            }

            if (timestampWorkflow.Proof.Confirmations > 0)
            {
                if (trust.Timestamps == null)
                    trust.Timestamps = new List<Timestamp>();

                var stamp = new Timestamp
                {
                    Algorithm = timestampWorkflow.Proof.Blockchain,
                    Receipt = proof.Receipt
                };

                _trustDBService.DBContext.Trusts.Update(trust);

                _updatedTrusts++;
            }
        }
    }
}
