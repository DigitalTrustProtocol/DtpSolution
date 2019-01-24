using DtpCore.Model;

namespace DtpCore.Interfaces
{
    public interface IClaimBanListService
    {
        bool Ensure(Claim claim);
        bool IsBanned(Claim claim);
        bool IsBanClaim(Claim claim);
        void Clean();
    }
}