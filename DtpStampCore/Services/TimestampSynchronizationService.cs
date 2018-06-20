using DtpStampCore.Interfaces;

namespace DtpStampCore.Services
{
    public class TimestampSynchronizationService : ITimestampSynchronizationService
    {

        public int CurrentWorkflowID
        {
            get;
            set;
        }

        public TimestampSynchronizationService()
        {
            CurrentWorkflowID = 0;
        }
    }
}
