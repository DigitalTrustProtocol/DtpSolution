using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IPackageBinary
    {
        IClaimBinary ClaimBinary { get; }
        byte[] GetIdSource(Package package);
        byte[] GetPackageBinary(Package package);
    }
}