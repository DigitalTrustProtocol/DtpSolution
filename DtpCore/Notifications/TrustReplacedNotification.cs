using DtpCore.Extensions;
using DtpCore.Model;
using MediatR;

namespace DtpCore.Notifications
{
    public class TrustReplacedNotification : INotification
    {
        public Trust Trust { get; set; }

        public override string ToString()
        {
            return $"Trust {Trust?.Id.ToHex()} is replaced";
        }
    }
}
