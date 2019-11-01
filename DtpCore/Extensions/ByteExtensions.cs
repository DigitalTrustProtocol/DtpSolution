using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtpCore.Extensions
{
    public static class ByteExtensions
    {

        public static byte[] Combine2(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        public static byte[] Combine(this byte[] left, byte[] right)
        {
            if (left == null)
                if (right != null)
                    return right;
                else
                    return null;

            var s = new List<byte>(left);
            if(right != null)
                s.AddRange(right);
            return s.ToArray();
        }

        public static bool Equals(this byte[] source, byte[] target)
        {
            return source.Compare(target) == 0;
        }

        public static int Compare(this byte[] source, byte[] target)
        {
            if (source == null && target == null)
                return 0;

            if (source.Length != target.Length)
                throw new ApplicationException("Byte arrays has to have the same length");

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] > target[i])
                    return 1;

                if (source[i] < target[i])
                    return -1;
            }

            return 0;
        }

        public static string ToHex(this byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

    }

}
