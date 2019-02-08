using MediatR;
using DtpCore.Notifications;
using DtpCore.Model;

namespace DtpPackageCore.Commands
{
    public class GetPackageCommand : IRequest<Package>
    {
        public int DatabaseID { get; set; }
        public byte[] ID { get; set; }

        public bool IncludeClaims { get; set; }

        public GetPackageCommand()
        {
            IncludeClaims = true;
        }
    }
}
