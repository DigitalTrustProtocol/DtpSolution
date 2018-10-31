using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface ITrustBinary
    {
        byte[] GetIssuerBinary(Trust trust);
        byte[] GetPackageBinary(Package package, byte[] merkleRoot);
    }
}