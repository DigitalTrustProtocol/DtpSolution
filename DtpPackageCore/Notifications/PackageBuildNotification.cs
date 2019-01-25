using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageBuildNotification : INotification
    {
        public Package Package { get; }

        public PackageBuildNotification(Package package)
        {
            Package = package;
        }

    }
}
