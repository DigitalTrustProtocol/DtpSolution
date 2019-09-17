using System.IO;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Interfaces;
using DtpCore.IO;

namespace DtpCore.Strategy
{
    public class ClaimBinary : IClaimBinary
    {
        /// <summary>
        /// Get the Id source data from the claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public byte[] GetIdSource(Claim claim)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new CompressedBinaryWriter(ms);
                BuildSource(claim, bw);
                bw.Flush();

                return ms.ToArray();
            }
        }
               
        /// <summary>
        /// Gets the full binary data of the claim. 
        /// Cannot not be used for ID calculation, as it contains the proof from Issuer and subject.
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public byte[] Serialize(Claim claim)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new CompressedBinaryWriter(ms);
                BuildSource(claim, bw);

                bw.Write(claim.Issuer.Proof);
                bw.Write(claim.Subject.Proof);
                bw.Write(claim.TemplateId);

                // ID is not added as it is dependen on the issuer/subject type 

                foreach (var timestamp in claim.Timestamps)
                {

                    // TODO: Need to add timestamps here!
                }
                bw.Flush();

                return ms.ToArray();
            }
        }

        public Claim Deserialize(byte[] data)
        {
            // TODO: Implement
            return null;
        }

        /// <summary>
        /// Get the binary data for ID calculation. Do not include the proof of Issuer and subject.
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="bw"></param>
        private static void BuildSource(Claim claim, CompressedBinaryWriter bw)
        {
            bw.Write(claim.Type.ToLowerSafe()); // First type!

            bw.Write(claim.Issuer.Type.ToLowerSafe());
            bw.Write(claim.Issuer.Id);
            bw.Write(claim.Issuer.Path);

            bw.Write(claim.Subject.Type.ToLowerSafe());
            bw.Write(claim.Subject.Id);
            bw.Write(claim.Subject.Path);

            bw.Write(claim.Value);

            bw.Write(claim.Scope);
            bw.Write(claim.Note);
            bw.Write(claim.Created);
            bw.Write(claim.Activate);
            bw.Write(claim.Expire);
        }

    }
}
