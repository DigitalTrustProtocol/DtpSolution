using DtpCore.Interfaces;

namespace DtpStampCore.Interfaces
{
    public interface ILocalTimestampStep : IWorkflowStep
    {
        int RetryAttempts { get; set; }
    }
}