using NBitcoin.Crypto;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DtpCore.Interfaces;
using DtpGraphCore.Enumerations;

namespace DtpGraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GraphClaim //: IGraphClaim
    {
        public int Index;
        public short Cost;  // cost of following the trust, lower the better
        public int Scope; // scope of the trust
        public int Type; // Type of the trust
        public int Attributes; // Claims 
        public int Note;
        public ClaimFlags Flags;

        //public int Activate; // When to begin consider the trust. This will be moved to an external event, do not check every time traversing the Graph
        //public int Expire;    // When the trust expire, This will be moved to an external event

        public string ID()
        {
            return $"T:{Type}:{Scope}:{Attributes}:{Note}";
        }

        //public static byte[] ByteID(string id)
        //{
        //    var data = Encoding.UTF8.GetBytes(id);
        //    return Hashes.RIPEMD160(data, data.Length);
        //}



    }
}
