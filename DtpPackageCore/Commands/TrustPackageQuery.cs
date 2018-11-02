using DtpCore.Commands;
using DtpCore.Interfaces;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class TrustPackageQuery : QueryCommand, IRequest<IPaginatedList<Package>>
    {
        public int? DatabaseID { get; set; }
    }
}
