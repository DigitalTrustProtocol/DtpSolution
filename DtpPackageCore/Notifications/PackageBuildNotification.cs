using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageBuildNotification : INotification
    {
        public Package TrustPackage { get; }

        public PackageBuildNotification(Package trustPackage)
        {
            TrustPackage = trustPackage;
        }

    }
}
