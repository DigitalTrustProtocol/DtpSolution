using DtpGraphCore.Model;
using DtpGraphCore.Services;

namespace DtpGraphCore.Interfaces
{
    public interface IGraphQueryService
    {
        IGraphTrustService TrustService { get; }

        QueryContext Execute(QueryRequest query);
    }
}