using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class TrustPackageBuildNotification : INotification
    {
        public Package TrustPackage { get; set; }

        public TrustPackageBuildNotification(Package trustPackage)
        {
            TrustPackage = trustPackage;
        }
    }
}
