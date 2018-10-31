using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class ConfirmationUpdatedNotification : INotification
    {
        public Timestamp Stamp { get; set; }

        public ConfirmationUpdatedNotification(Timestamp stamp)
        {
            Stamp = stamp;
        }
    }
}
