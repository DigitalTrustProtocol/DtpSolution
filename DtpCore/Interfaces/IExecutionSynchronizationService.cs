using System.Collections.Concurrent;

namespace DtpCore.Interfaces
{
    public interface IExecutionSynchronizationService
    {
        ConcurrentDictionary<int, IWorkflowContext> Workflows { get; set; }
    }
}
