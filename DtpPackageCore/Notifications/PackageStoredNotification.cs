using DtpCore.Model;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageStoredNotification : INotification
    {
        public string File { get; }
        public Package Package { get; }

        public PackageStoredNotification(string file, Package package)
        {
            File = file;
            Package = package;
        }
    }
}
