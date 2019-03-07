using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Commands
{
    public class GetTimestampCommand : IRequest<Timestamp>
    {
        public byte[] Source { get; }
        public bool IncludeProof { get; }

        public GetTimestampCommand(byte[] source, bool includeProof = false)
        {
            Source = source;
            IncludeProof = includeProof;
        }
    }
}
