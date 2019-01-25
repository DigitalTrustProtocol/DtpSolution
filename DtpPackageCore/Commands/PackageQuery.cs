using DtpCore.Commands;
using DtpCore.Interfaces;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class PackageQuery : QueryCommand, IRequest<IPaginatedList<Package>>
    {
        public PackageQuery(int? databaseID = null, bool includeClaims = false)
        {
            DatabaseID = databaseID;
            IncludeClaims = includeClaims;
        }

        public int? DatabaseID { get; }
        public bool IncludeClaims { get; }



    }
}
