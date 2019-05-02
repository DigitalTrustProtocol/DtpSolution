using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DtpCore.IO
{
    public class CompressedBinaryReader : BinaryReader
    {
        public CompressedBinaryReader(Stream stream) : base(stream) { }
        public new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

        public byte[] ReadBytes()
        {
            var length = Read7BitEncodedInt();
            return base.ReadBytes(length);
        }
    }
}
