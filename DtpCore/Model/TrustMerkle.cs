using DtpCore.Interfaces;

namespace DtpCore.Model
{
    public class TrustMerkle : ITrustMerkle
    {
        public string Merkle { get; set; }
        public string Hash { get; set; }
    }
}
