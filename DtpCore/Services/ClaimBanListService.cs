using DtpCore.Builders;
using DtpCore.Interfaces;
using DtpCore.Model;
using System;
using System.Collections.Generic;

namespace DtpCore.Services
{
    public class ClaimBanListService : IClaimBanListService
    {
        public static Dictionary<string, Claim> BanList = new Dictionary<string, Claim>(1000, StringComparer.Ordinal);

        private static object lockObj = new object();

        public bool IsBanClaim(Claim claim)
        {
            return (claim.Type == PackageBuilder.REMOVE_CLAIMS_DTP1);
        }

        public bool IsBanned(Claim claim)
        {
            if (IsBanClaim(claim))
                return false; // Do not check for ban claims.

            if (BanList.TryGetValue(claim.Issuer.Id, out Claim banClaim))
            {
                // If the ban claim is newer then the result is true.
                if (banClaim.Created >= claim.Created)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Add the claim to the ban list. Replace existing claim if newer.
        /// Only claims that are defined as Ban claims are allowed into the list.
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public bool Ensure(Claim claim)
        {
            // Make sure that only ban claims get added to the list.
            if (!IsBanClaim(claim))
                return false; // Non ban claim.

            if (BanList.TryGetValue(claim.Issuer.Id, out Claim existingClaim))
            {
                // Make sure that an old claim cannot replace a newer one.
                if (existingClaim.Created >= claim.Created)
                    return false;
            }

            lock (lockObj) {
                BanList[claim.Issuer.Id] = claim;
            }

            return true;
        }

    }
}
