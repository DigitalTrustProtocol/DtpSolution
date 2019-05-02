using System.IO;
using DtpCore.Extensions;
using DtpCore.Model;
using DtpGraphCore.Model;
using DtpGraphCore.Interfaces;
using DtpCore.IO;

namespace DtpCore.Strategy
{
    public class QueryRequestBinary : IQueryRequestBinary
    {
        /// <summary>
        /// Get the Id source data from the claim
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        public byte[] GetIdSource(QueryRequest queryRequest)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new CompressedBinaryWriter(ms);
                BuildSource(queryRequest, bw);
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
        public byte[] Serialize(QueryRequest queryRequest)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new CompressedBinaryWriter(ms);

                BuildSource(queryRequest, bw);
                bw.Write(queryRequest.Issuer.Proof);

                bw.Flush();
                
                return ((MemoryStream)bw.BaseStream).ToArray();
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
        private static void BuildSource(QueryRequest queryRequest, CompressedBinaryWriter bw)
        {
            
            bw.Write(queryRequest.Issuer.Type.ToLowerSafe());
            bw.Write(queryRequest.Issuer.Id);

            bw.Write(queryRequest.Scope);
            bw.Write((byte)queryRequest.Flags);

            bw.Write(queryRequest.Types.Count);
            foreach (var type in queryRequest.Types)
            {
                bw.Write(type);
            }

            bw.Write(queryRequest.Subjects.Count);
            foreach (var subject in queryRequest.Subjects)
            {
                bw.Write(subject);
            }
        }

    }
}
