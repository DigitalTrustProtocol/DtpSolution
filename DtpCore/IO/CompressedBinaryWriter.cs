using System.Diagnostics;
using System.IO;

namespace DtpCore.IO
{
    public class CompressedBinaryWriter : BinaryWriter
    {
        public CompressedBinaryWriter(Stream stream) : base(stream) { }
        public new long Write7BitEncodedInt(int i)
        {
            var start = base.BaseStream.Length;
            base.Write7BitEncodedInt(i);
            long count = base.BaseStream.Length - start;
            //Trace.WriteLine($"Value: {i} are using {count} bytes");
            return count;
        }

        public new long Write(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Write7BitEncodedInt(0);
                return 1;
            }

            // Needs to be dynamic, so its possible to define values higher than 127
            // Currently per default, a property cannot be more than 127 bytes long. 
            // This is limit the possible size of each claim property.
            var count = Write7BitEncodedInt(bytes.Length);
            base.Write(bytes);
            return count + bytes.LongLength;
        }

        public new long Write(int value)
        {
            return Write7BitEncodedInt(value);
        }

        public new long Write(uint value)
        {
            return Write7BitEncodedInt((int)value);
        }

        public new long Write(string text)
        {
            if (text == null || text.Length == 0)
            {
                Write7BitEncodedInt(0);
                return 1;
            }

            var start = base.BaseStream.Length;
            base.Write(text);
            return base.BaseStream.Length - start;
        }

    }
}
