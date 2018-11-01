using Microsoft.Extensions.DependencyInjection;
using System;
using DtpCore.Services;
using DtpStampCore.Interfaces;
using DtpStampCore.Workflows;
using System.Linq;
using System.Collections.Generic;
using DtpCore.Interfaces;
using DtpCore.Enumerations;
using DtpCore.Extensions;
using Microsoft.Extensions.Configuration;
using DtpStampCore.Extensions;

namespace DtpStampCore.Services
{
    public class TimestampWorkflowService : ITimestampWorkflowService
    {

        public IWorkflowService WorkflowService { get; } 
        private ITrustDBService _trustDBService;
        private ITimestampSynchronizationService _timestampSynchronizationService;
        private IConfiguration _configuration;

        public TimestampWorkflowService(IWorkflowService workflowService, ITrustDBService trustDBService, ITimestampSynchronizationService timestampSynchronizationService, IConfiguration configuration)
        {
            WorkflowService = workflowService;
            _trustDBService = trustDBService;
            _timestampSynchronizationService = timestampSynchronizationService;
            _configuration = configuration;
        }

        public int CountCurrentProofs()
        {
            return _trustDBService.Timestamps.Where(p => p.WorkflowID == _timestampSynchronizationService.CurrentWorkflowID).Count();
        }

        public void CreateAndExecute()
        {
            var oldID = _timestampSynchronizationService.CurrentWorkflowID;

            CreateTimestampWorkflow();

            if (oldID == 0)
                return;

            // Activate the previous workflow
            var oldWf = WorkflowService.Load<ProcessProofWorkflow>(oldID);
            if(oldWf != null)
            {
                oldWf.Container.State = WorkflowStatusType.Starting.ToString();
                oldWf.Container.NextExecution = DateTime.Now.ToUnixTime();
                WorkflowService.Save(oldWf);
            }
        }

        public void EnsureTimestampScheduleWorkflow()
        {
            WorkflowService.EnsureWorkflow<CreateProofWorkflow>();
        }

        public void CreateTimestampWorkflow()
        {
            var wf = WorkflowService.Create<ProcessProofWorkflow>();
            wf.Container.State = WorkflowStatusType.Waiting.ToString();
            wf.Container.NextExecution = DateTime.Now.AddSeconds(_configuration.TimestampInterval()).ToUnixTime();
            WorkflowService.Save(wf);
            _timestampSynchronizationService.CurrentWorkflowID = wf.Container.DatabaseID;
        }

    }
}
