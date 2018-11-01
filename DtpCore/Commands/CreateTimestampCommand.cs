using DtpCore.Model;
using MediatR;

namespace DtpCore.Commands
{
    public class CreateTimestampCommand : IRequest<Timestamp>
    {
        public byte[] Source { get; set; }
    }
}
