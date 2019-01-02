using DtpGraphCore.Model;
using DtpGraphCore.Services;

namespace DtpGraphCore.Interfaces
{
    public interface IGraphQueryService
    {
        IGraphClaimService TrustService { get; }

        QueryContext Execute(QueryRequest query);
    }
}