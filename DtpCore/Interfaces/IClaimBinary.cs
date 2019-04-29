using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IClaimBinary
    {
        byte[] GetIdSource(Claim trust);
        byte[] Serialize(Claim trust);
    }
}