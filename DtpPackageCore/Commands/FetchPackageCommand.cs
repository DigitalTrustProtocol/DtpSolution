using DtpCore.Model;
using DtpCore.Notifications;
using DtpPackageCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class FetchPackageCommand : IRequest<Package>
    {
        public string File { get; }

        public FetchPackageCommand(string file)
        {
            File = file;
        }


    }
}
