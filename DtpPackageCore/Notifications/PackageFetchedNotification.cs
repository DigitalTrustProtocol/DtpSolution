using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageFetchedNotification : INotification
    {
        public string File { get; }
        public Package Package { get; }

        public PackageFetchedNotification(string file, Package package)
        {
            File = file;
            Package = package;
        }

    }
}
