using DtpCore.Extensions;
using DtpCore.IO;
using Newtonsoft.Json;
using System.IO;

namespace DtpPackageCore.Model
{
    public class PackageMessage
    {
        /// <summary>
        /// A path to an existing file, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
        //  or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"
        /// </summary>
        public string File { get; set; }

        public string Scope { get; set; }

        //public int Length { get; set; }

        public string ServerId { get; set; }

        public byte[] ServerSignature { get; set; }

        public PackageMessage()
        {
            
        }

        public byte[] ToBinary()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new CompressedBinaryWriter(ms);

                bw.Write(File);
                bw.Write(Scope);
                bw.Write(ServerId);
                bw.Flush();

                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
