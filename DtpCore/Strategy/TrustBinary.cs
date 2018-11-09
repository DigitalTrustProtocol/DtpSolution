using System.IO;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Interfaces;

namespace DtpCore.Strategy
{
    public class TrustBinary : ITrustBinary
    {
        public TrustBinary()
        {
        }

        public byte[] GetIssuerBinary(Trust trust)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (trust.Issuer != null)
                {
                    ms.WriteString(trust.Issuer.Type.ToLowerSafe());
                    ms.WriteString(trust.Issuer.Address);
                }

                if (trust.Subject != null)
                {
                    ms.WriteString(trust.Subject.Type.ToLowerSafe());
                    ms.WriteString(trust.Subject.Address);
                }

                ms.WriteString(trust.Type.ToLowerSafe());
                ms.WriteString(trust.Claim);

                if (trust.Scope != null)
                {
                    ms.WriteString(trust.Scope);
                }

                ms.WriteInteger(trust.Created);
                ms.WriteInteger(trust.Activate);
                ms.WriteInteger(trust.Expire);

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
                    ms.WriteString(package.Server.Address);
                }
                
                return ms.ToArray();
            }
        }
    }
}
