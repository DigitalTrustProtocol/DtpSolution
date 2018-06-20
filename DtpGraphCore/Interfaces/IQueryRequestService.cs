using DtpGraphCore.Model;

namespace DtpGraphCore.Interfaces
{
    public interface IQueryRequestService
    {
        void Verify(QueryRequest query);
    }
}