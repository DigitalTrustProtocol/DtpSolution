using System.IO;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpCore.Interfaces;


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
                BuildSource(claim, ms);

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
                BuildSource(claim, ms);

                ms.LWriteBytes(claim.Issuer.Proof);
                ms.LWriteBytes(claim.Subject.Proof);
                ms.LWriteBytes(claim.Root);
                ms.LWriteInteger(claim.TemplateId);

                // ID is not added as it is dependen on the issuer/subject type 

                foreach (var timestamp in claim.Timestamps)
                {

                    // TODO: Need to add timestamps here!
                }

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
        /// <param name="ms"></param>
        private static void BuildSource(Claim claim, MemoryStream ms)
        {
            ms.LWriteString(claim.Type.ToLowerSafe()); // First type!

            ms.LWriteString(claim.Issuer.Type.ToLowerSafe());
            ms.LWriteString(claim.Issuer.Id);

            ms.LWriteString(claim.Subject.Type.ToLowerSafe());
            ms.LWriteString(claim.Subject.Id);

            ms.LWriteString(claim.Value);

            ms.LWriteString(claim.Scope);
            ms.LWriteString(claim.Metadata);
            ms.LWriteInteger(claim.Created);
            ms.LWriteInteger(claim.Activate);
            ms.LWriteInteger(claim.Expire);
        }

    }
}
