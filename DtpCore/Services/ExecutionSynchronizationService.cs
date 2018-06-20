using System.Collections.Concurrent;
using DtpCore.Interfaces;

namespace DtpCore.Services
{
    public class ExecutionSynchronizationService : IExecutionSynchronizationService
    {
        public ConcurrentDictionary<int, IWorkflowContext> Workflows { get; set; }

        public ExecutionSynchronizationService()
        {
            Workflows = new ConcurrentDictionary<int, IWorkflowContext>();
        }
    }
}
