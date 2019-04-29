using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.Model;
using System.IO;

namespace DtpCore.Strategy
{
    public class PackageBinary : IPackageBinary
    {

        public IClaimBinary ClaimBinary { get; }

        public PackageBinary(IClaimBinary claimBinary)
        {
            ClaimBinary = claimBinary;
        }

        /// <summary>
        ///  Not implemented!
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public byte[] GetPackageBinary(Package package)
        {

            return null;
        }


        public byte[] GetIdSource(Package package)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.LWriteString(package.Algorithm.ToLowerSafe());
                ms.LWriteString(package.Server.Type.ToLowerSafe());
                ms.LWriteString(package.Server.Id);
                ms.LWriteInteger(package.Created);
                //ms.WriteClaimString(package.Server.Id); // Id can always be calculated

                return ms.ToArray();
            }
        }

    }
}
