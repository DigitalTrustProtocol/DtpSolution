using DtpCore.Interfaces;

namespace DtpStampCore.Interfaces
{
    public interface IAddressVerifyStep : IWorkflowStep
    {
        int RetryAttempts { get; set; }
    }
}