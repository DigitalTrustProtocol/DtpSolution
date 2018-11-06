using DtpCore.Commands;
using DtpCore.Interfaces;
using DtpCore.Model;
using MediatR;

namespace DtpPackageCore.Commands
{
    public class TrustPackageQuery : QueryCommand, IRequest<IPaginatedList<Package>>
    {
        public TrustPackageQuery(int? databaseID = null, bool includeTrusts = false)
        {
            DatabaseID = databaseID;
            IncludeTrusts = includeTrusts;
        }

        public int? DatabaseID { get; }
        public bool IncludeTrusts { get; }



    }
}
