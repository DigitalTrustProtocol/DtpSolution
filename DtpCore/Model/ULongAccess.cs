using System;
using System.Runtime.InteropServices;

namespace DtpCore.Model
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ULongAccess
    {
        [FieldOffset(0)]
        public UInt64 Value;

        [FieldOffset(0)]
        public UInt32 High;

        [FieldOffset(4)]
        public UInt32 Low;
    }
}
