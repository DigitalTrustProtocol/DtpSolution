using DtpCore.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DtpCore.Extensions;

namespace UnitTest.DtpCore.IO
{
    [TestClass]
    public class CompressedBinaryWriterTest
    {
        [TestMethod]
        public void WriteLength()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new CompressedBinaryWriter(stream);

                Assert.IsTrue(writer.Write7BitEncodedInt(100) == 1);
                Assert.IsTrue(writer.Write7BitEncodedInt(-100) == 5); // 5 Bytes
                Assert.IsTrue(writer.Write7BitEncodedInt(127) == 1);
                Assert.IsTrue(writer.Write7BitEncodedInt(128) == 2);
                Assert.IsTrue(writer.Write7BitEncodedInt(1000) == 2);
                Assert.IsTrue(writer.Write7BitEncodedInt(10000)== 2);
                Assert.IsTrue(writer.Write7BitEncodedInt(100000) == 3);
                Assert.IsTrue(writer.Write7BitEncodedInt(1000000) == 3);
                Assert.IsTrue(writer.Write7BitEncodedInt(-1000000) == 5);
                Assert.IsTrue(writer.Write7BitEncodedInt(1556797218) == 5);
                

                stream.Position = 0;

                var reader = new CompressedBinaryReader(stream);

                Assert.IsTrue(reader.Read7BitEncodedInt() == 100);
                Assert.IsTrue(reader.Read7BitEncodedInt() == -100);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 127);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 128);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 1000);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 10000);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 100000);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 1000000);
                Assert.IsTrue(reader.Read7BitEncodedInt() == -1000000);
                Assert.IsTrue(reader.Read7BitEncodedInt() == 1556797218);
            }
        }

        [TestMethod]
        public void WriteBytes()
        {
            using (var stream = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes("Hello");
                var writer = new CompressedBinaryWriter(stream);

                var total = writer.Write(bytes);
                stream.Position = 0;

                var reader = new CompressedBinaryReader(stream);

                var readBytes = reader.ReadBytes();
                Assert.IsTrue(readBytes.Length == bytes.Length);
                Assert.IsTrue(ByteExtensions.Equals(readBytes, bytes));
                Assert.IsTrue(stream.Length == total);
            }
        }

        [TestMethod]
        public void WriteString()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new CompressedBinaryWriter(stream);
                var source = "Hello";
                writer.Write(source);
                var total = stream.Length;
                Console.WriteLine($"Text length: {source.Length} - Stream length: {stream.Length}");
                stream.Position = 0;

                var reader = new CompressedBinaryReader(stream);

                var target = reader.ReadString();
                Assert.IsTrue(source == target);
                Assert.IsTrue(stream.Length == total);
            }
        }

    }
}
