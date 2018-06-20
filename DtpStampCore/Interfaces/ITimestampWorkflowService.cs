using System.Collections.Generic;
using DtpCore.Services;
using DtpStampCore.Workflows;

namespace DtpStampCore.Interfaces
{
    public interface ITimestampWorkflowService
    {
        IWorkflowService WorkflowService { get; }
        int CountCurrentProofs();
        void CreateAndExecute();
        void EnsureTimestampScheduleWorkflow();
        void CreateTimestampWorkflow();
    }
}