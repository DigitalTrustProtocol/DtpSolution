using DtpCore.Model;
using DtpGraphCore.Model;

namespace DtpGraphCore.Interfaces
{
    public interface IQueryRequestBinary
    {
        byte[] GetIdSource(QueryRequest queryRequest);
        byte[] Serialize(QueryRequest queryRequest);
    }
}