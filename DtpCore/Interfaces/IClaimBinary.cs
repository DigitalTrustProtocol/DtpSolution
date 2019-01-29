using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IClaimBinary
    {
        byte[] GetIssuerBinary(Claim trust);
        byte[] GetPackageBinary(Package package);
    }
}