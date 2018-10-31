using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class TrustPackageCreatedNotification : INotification
    {
        public Package TrustPackage { get; set; }

        public TrustPackageCreatedNotification(Package trustPackage)
        {
            TrustPackage = trustPackage;
        }
    }
}
