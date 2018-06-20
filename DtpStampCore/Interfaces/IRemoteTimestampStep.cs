using DtpCore.Interfaces;

namespace DtpStampCore.Interfaces
{
    public interface IRemoteTimestampStep : IWorkflowStep
    {
        int RetryAttempts { get; set; }
    }
}