using System.Runtime.InteropServices;
using DtpGraphCore.Enumerations;

namespace DtpGraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GraphClaim //: IGraphClaim
    {
        public int Index;
        public int Type; // Type of the trust
        public int Scope; // scope of the trust
        public int Attributes; // Claims 
        public ClaimFlags Flags;

        public string ID()
        {
            return $"T:{Type}:{Scope}:{Attributes}";
        }
    }
}
