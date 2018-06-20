using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace DtpCore.Collections.Generic
{

    public static class ByteComparer
    {
        private class StandardComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                    return left == right;

                if (ReferenceEquals(left, right))
                    return true;

                if (left.Length != right.Length)
                    return false;

                int index = 0;
                while (index < left.Length)
                {
                    if (left[index] != right[index])
                        return false;
                    index++;
                }
                return true;
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }

        private class ByteComparerImplementation : IComparer<byte[]>
        {
            public int Compare(byte[] x, byte[] y)
            {
                var len = Math.Min(x.Length, y.Length);
                for (var i = 0; i < len; i++)
                {
                    var c = x[i].CompareTo(y[i]);
                    if (c != 0)
                    {
                        return c;
                    }
                }

                return x.Length.CompareTo(y.Length);
            }
        }

        public static readonly IComparer<byte[]> Compare = new ByteComparerImplementation();


        public static readonly IEqualityComparer<byte[]> Standard = new StandardComparer();
    }
}
