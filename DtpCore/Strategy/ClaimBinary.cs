using System.IO;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Interfaces;

namespace DtpCore.Strategy
{
    public class ClaimBinary : IClaimBinary
    {
        public ClaimBinary()
        {
        }

        public byte[] GetIssuerBinary(Claim claim)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (claim.Issuer != null)
                {
                    ms.WriteString(claim.Issuer.Type.ToLowerSafe());
                    ms.WriteString(claim.Issuer.Id);
                }

                if (claim.Subject != null)
                {
                    ms.WriteString(claim.Subject.Type.ToLowerSafe());
                    ms.WriteString(claim.Subject.Id);
                }

                ms.WriteString(claim.Type.ToLowerSafe());
                ms.WriteString(claim.Value);

                if (claim.Scope != null)
                {
                    ms.WriteString(claim.Scope);
                }

                ms.WriteInteger(claim.Created);
                ms.WriteInteger(claim.Activate);
                ms.WriteInteger(claim.Expire);

                return ms.ToArray();
            }
        }


        public byte[] GetPackageBinary(Package package, byte[] merkleRoot)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (merkleRoot != null)
                    ms.WriteBytes(merkleRoot);

                if (package == null)
                    return ms.ToArray();

                if(package.Algorithm != null)
                {
                    ms.WriteString(package.Algorithm);
                }

                ms.WriteInteger(package.Created);

                if(package.Server != null)
                {
                    ms.WriteString(package.Server.Type.ToLowerSafe());
                    ms.WriteString(package.Server.Id);
                }
                
                return ms.ToArray();
            }
        }
    }
}
