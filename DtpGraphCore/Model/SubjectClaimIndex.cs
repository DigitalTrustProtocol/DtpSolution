using System;
using System.Runtime.InteropServices;

namespace DtpGraphCore.Model
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SubjectClaimIndex
    {
        [FieldOffset(0)]
        public Int64 Value;
        [FieldOffset(0)]
        public Int32 Type;
        [FieldOffset(4)]
        public Int32 Scope;

        public SubjectClaimIndex(Int64 value)
        {
            Scope = 0;
            Type = 0;
            Value = value;
        }

        public SubjectClaimIndex(Int32 scope, Int32 index)
        {
            Value = 0;
            Scope = scope;
            Type = index;
        }
    }
}
