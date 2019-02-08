using DtpCore.Model;
using MediatR;

namespace DtpStampCore.Commands
{
    public class CreateTimestampCommand : IRequest<Timestamp>
    {
        public byte[] Source { get; set; }
    }
}
