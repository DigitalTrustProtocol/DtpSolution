using DtpCore.Extensions;
using DtpCore.Interfaces;
using DtpCore.IO;
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

        public byte[] GetIdSource(Package package)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new CompressedBinaryWriter(ms);

                bw.Write(package.Type.ToLowerSafe());
                bw.Write(package.Server.Type.ToLowerSafe());
                bw.Write(package.Server.Id);
                bw.Write(package.Server.Path);
                bw.Write(package.Created);
                //ms.WriteClaimString(package.Server.Id); // Id can always be calculated
                bw.Flush();

                return ms.ToArray();
            }
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

    }
}
