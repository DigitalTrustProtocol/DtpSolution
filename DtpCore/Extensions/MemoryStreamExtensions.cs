using System;
using System.IO;
using System.Text;

namespace DtpCore.Extensions
{
    public static class MemoryStreamExtensions
    {
        public static void LWriteBytes(this MemoryStream ms, byte[] data)
        {
            // If there is no data, then only define the Length value and set it to 0.
            if (data == null || data.Length == 0)
            {
                ms.WriteByte(0);
                return;
            }

            // Needs to be dynamic, so its possible to define values higher than 256
            // Currently per default, a property cannot be more than 127 bytes long. 
            // This is limit the possible size of each claim.
            ms.WriteByte((byte)data.Length); // Write length
            ms.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Write a string in UTF8 format to the memoryStream
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="text"></param>
        public static void LWriteString(this MemoryStream ms, string text)
        {
            if(text == null)
            {
                ms.WriteByte(0);
                return;
            }

            var data = Encoding.UTF8.GetBytes(text);
            ms.LWriteBytes(data);
        }

        public static void WriteClaimInteger(this MemoryStream ms, int num)
        {
            if (num == 0)
            {
                ms.WriteByte(0);
                return;
            }

            var bytes = BitConverter.GetBytes(num);
            ms.LWriteBytes(bytes);
        }

        public static void LWriteInteger(this MemoryStream ms, uint num)
        {
            if(num == 0)
            {
                ms.WriteByte(0);
                return;
            }

            var bytes = BitConverter.GetBytes(num);
            ms.LWriteBytes(bytes);
        }

        public static void WriteClaimLong(this MemoryStream ms, long num)
        {
            if (num == 0)
            {
                ms.WriteByte(0);
                return;
            }

            var bytes = BitConverter.GetBytes(num);
            ms.LWriteBytes(bytes);
        }
    }
}
