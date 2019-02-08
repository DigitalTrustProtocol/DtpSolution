using DtpCore.Enumerations;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.ViewModel;
using MediatR;

namespace DtpPackageCore.Notifications
{
    public class PackageAddedNotification : INotification
    {
        
        public Package Package { get; set; }
        //public PackageAddedResult Result{ get; set; }

        public override string ToString()
        {
            return $"Package {Package?.Id.ToHex()} added";
        }
    }
}
