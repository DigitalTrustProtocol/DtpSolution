using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

/// <summary>
/// Giving up some security and making the private m_array accessiable from Extensions methods, enables a performance up to a factor of 5.
/// </summary>
namespace DtpCore.Collections
{
    /// <summary>
    /// Implemented the Get and Set as fast metods enabling AggressiveInlining and disabling range check.
    /// </summary>
    public static class BitArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetFast(this BitArrayFast bitArray, int index)
        {
            return (bitArray.m_array[index / 32] & (1 << (index % 32))) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFast(this BitArrayFast bitArray, int index, bool value)
        {
            if (value)
                bitArray.m_array[index / 32] |= (1 << (index % 32));
            else
                bitArray.m_array[index / 32] &= ~(1 << (index % 32));
        }
    }

    // ==++==
    // 
    //   Copyright (c) Microsoft Corporation.  All rights reserved.
    // 
    // ==--==
    /*=============================================================================
    **
    ** Class: BitArray
    **
    ** <OWNER>Microsoft</OWNER>
    **
    **
    ** Purpose: The BitArray class manages a compact array of bit values.
    **
    **
    =============================================================================*/


    // A vector of bits.  Use this to store bits efficiently, without having to do bit 
    // shifting yourself.
    //[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class BitArrayFast : ICollection, ICloneable
    {
        private BitArrayFast()
        {
        }

        /*=========================================================================
        ** Allocates space to hold length bit values. All of the values in the bit
        ** array are set to false.
        **
        ** Exceptions: ArgumentException if length < 0.
        =========================================================================*/
        public BitArrayFast(int length)
            : this(length, false)
        {
        }

        /*=========================================================================
        ** Allocates space to hold length bit values. All of the values in the bit
        ** array are set to defaultValue.
        **
        ** Exceptions: ArgumentOutOfRangeException if length < 0.
        =========================================================================*/
        public BitArrayFast(int length, bool defaultValue)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_NeedNonNegNum");
            }
            Contract.EndContractBlock();

            m_array = new int[GetArrayLength(length, BitsPerInt32)];
            m_length = length;

            // We will manually set the default value later.
            //int fillValue = defaultValue ? unchecked(((int)0xffffffff)) : 0;
            //for (int i = 0; i < m_array.Length; i++)
            //{
            //    m_array[i] = fillValue;
            //}

            _version = 0;
        }



        /*=========================================================================
        ** Allocates space to hold the bit values in bytes. bytes[0] represents
        ** bits 0 - 7, bytes[1] represents bits 8 - 15, etc. The LSB of each byte
        ** represents the lowest index value; bytes[0] & 1 represents bit 0,
        ** bytes[0] & 2 represents bit 1, bytes[0] & 4 represents bit 2, etc.
        **
        ** Exceptions: ArgumentException if bytes == null.
        =========================================================================*/
        public BitArrayFast(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            Contract.EndContractBlock();
            // this value is chosen to prevent overflow when computing m_length.
            // m_length is of type int32 and is exposed as a property, so 
            // type of m_length can't be changed to accommodate.
            if (bytes.Length > Int32.MaxValue / BitsPerByte)
            {
                throw new ArgumentException("Argument_ArrayTooLarge");
            }

            m_array = new int[GetArrayLength(bytes.Length, BytesPerInt32)];
            m_length = bytes.Length * BitsPerByte;

            int i = 0;
            int j = 0;
            while (bytes.Length - j >= 4)
            {
                m_array[i++] = (bytes[j] & 0xff) |
                              ((bytes[j + 1] & 0xff) << 8) |
                              ((bytes[j + 2] & 0xff) << 16) |
                              ((bytes[j + 3] & 0xff) << 24);
                j += 4;
            }

            Contract.Assert(bytes.Length - j >= 0, "BitArray byteLength problem");
            Contract.Assert(bytes.Length - j < 4, "BitArray byteLength problem #2");

            switch (bytes.Length - j)
            {
                case 3:
                    m_array[i] = ((bytes[j + 2] & 0xff) << 16);
                    goto case 2;
                // fall through
                case 2:
                    m_array[i] |= ((bytes[j + 1] & 0xff) << 8);
                    goto case 1;
                // fall through
                case 1:
                    m_array[i] |= (bytes[j] & 0xff);
                    break;
            }

            _version = 0;
        }

        public BitArrayFast(bool[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            Contract.EndContractBlock();

            m_array = new int[GetArrayLength(values.Length, BitsPerInt32)];
            m_length = values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    m_array[i / 32] |= (1 << (i % 32));
            }

            _version = 0;

        }

        /*=========================================================================
        ** Allocates space to hold the bit values in values. values[0] represents
        ** bits 0 - 31, values[1] represents bits 32 - 63, etc. The LSB of each
        ** integer represents the lowest index value; values[0] & 1 represents bit
        ** 0, values[0] & 2 represents bit 1, values[0] & 4 represents bit 2, etc.
        **
        ** Exceptions: ArgumentException if values == null.
        =========================================================================*/
        public BitArrayFast(int[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            Contract.EndContractBlock();
            // this value is chosen to prevent overflow when computing m_length
            if (values.Length > Int32.MaxValue / BitsPerInt32)
            {
                throw new ArgumentException("Argument_ArrayTooLarge");
            }

            m_array = new int[values.Length];
            m_length = values.Length * BitsPerInt32;

            Array.Copy(values, m_array, values.Length);

            _version = 0;
        }

        /*=========================================================================
        ** Allocates a new BitArray with the same length and bit values as bits.
        **
        ** Exceptions: ArgumentException if bits == null.
        =========================================================================*/
        public BitArrayFast(BitArrayFast bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException("bits");
            }
            Contract.EndContractBlock();

            int arrayLength = GetArrayLength(bits.m_length, BitsPerInt32);
            m_array = new int[arrayLength];
            m_length = bits.m_length;

            Array.Copy(bits.m_array, m_array, arrayLength);

            _version = bits._version;
        }

        public bool this[int index]
        {
            get
            {
                return this.GetFast(index);
            }
            set
            {
                this.SetFast(index, value);
            }
        }

        /*=========================================================================
        ** Returns the bit value at position index.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public bool Get(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            }
            Contract.EndContractBlock();

            return (m_array[index / 32] & (1 << (index % 32))) != 0;
        }

        /*=========================================================================
        ** Sets the bit value at position index to value.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public void Set(int index, bool value)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            }
            Contract.EndContractBlock();

            if (value)
            {
                m_array[index / 32] |= (1 << (index % 32));
            }
            else
            {
                m_array[index / 32] &= ~(1 << (index % 32));
            }

            _version++;
        }

        /*=========================================================================
        ** Sets all the bit values to value.
        =========================================================================*/
        public void SetAll(bool value)
        {
            int fillValue = value ? unchecked(((int)0xffffffff)) : 0;
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] = fillValue;
            }

            _version++;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ANDed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitArrayFast And(BitArrayFast value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");
            Contract.EndContractBlock();

            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] &= value.m_array[i];
            }

            _version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitArrayFast Or(BitArrayFast value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");
            Contract.EndContractBlock();

            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] |= value.m_array[i];
            }

            _version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance XORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitArrayFast Xor(BitArrayFast value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");
            Contract.EndContractBlock();

            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] ^= value.m_array[i];
            }

            _version++;
            return this;
        }

        /*=========================================================================
        ** Inverts all the bit values. On/true bit values are converted to
        ** off/false. Off/false bit values are turned on/true. The current instance
        ** is updated and returned.
        =========================================================================*/
        public BitArrayFast Not()
        {
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] = ~m_array[i];
            }

            _version++;
            return this;
        }

        public int Length
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return m_length;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NeedNonNegNum");
                }
                Contract.EndContractBlock();

                int newints = GetArrayLength(value, BitsPerInt32);
                if (newints > m_array.Length || newints + _ShrinkThreshold < m_array.Length)
                {
                    // grow or shrink (if wasting more than _ShrinkThreshold ints)
                    int[] newarray = new int[newints];
                    Array.Copy(m_array, newarray, newints > m_array.Length ? m_array.Length : newints);
                    m_array = newarray;
                }

                if (value > m_length)
                {
                    // clear high bit values in the last int
                    int last = GetArrayLength(m_length, BitsPerInt32) - 1;
                    int bits = m_length % 32;
                    if (bits > 0)
                    {
                        m_array[last] &= (1 << bits) - 1;
                    }

                    // clear remaining int values
                    Array.Clear(m_array, last + 1, newints - last - 1);
                }

                m_length = value;
                _version++;
            }
        }

        // ICollection implementation
        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");

            if (array.Rank != 1)
                throw new ArgumentException("Arg_RankMultiDimNotSupported");

            Contract.EndContractBlock();

            if (array is int[])
            {
                Array.Copy(m_array, 0, array, index, GetArrayLength(m_length, BitsPerInt32));
            }
            else if (array is byte[])
            {
                int arrayLength = GetArrayLength(m_length, BitsPerByte);
                if ((array.Length - index) < arrayLength)
                    throw new ArgumentException("Argument_InvalidOffLen");

                byte[] b = (byte[])array;
                for (int i = 0; i < arrayLength; i++)
                    b[index + i] = (byte)((m_array[i / 4] >> ((i % 4) * 8)) & 0x000000FF); // Shift to bring the required byte to LSB, then mask
            }
            else if (array is bool[])
            {
                if (array.Length - index < m_length)
                    throw new ArgumentException("Argument_InvalidOffLen");

                bool[] b = (bool[])array;
                for (int i = 0; i < m_length; i++)
                    b[index + i] = ((m_array[i / 32] >> (i % 32)) & 0x00000001) != 0;
            }
            else
                throw new ArgumentException("Arg_BitArrayTypeUnsupported");
        }

        public int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return m_length;
            }
        }

        public Object Clone()
        {
            Contract.Ensures(Contract.Result<Object>() != null);
            Contract.Ensures(((BitArrayFast)Contract.Result<Object>()).Length == this.Length);

            BitArrayFast bitArray = new BitArrayFast(m_array);
            bitArray._version = _version;
            bitArray.m_length = m_length;
            return bitArray;
        }

        public Object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new BitArrayEnumeratorSimple(this);
        }

        // XPerY=n means that n Xs can be stored in 1 Y. 
        private const int BitsPerInt32 = 32;
        private const int BytesPerInt32 = 4;
        private const int BitsPerByte = 8;

        /// <summary>
        /// Used for conversion between different representations of bit array. 
        /// Returns (n+(div-1))/div, rearranged to avoid arithmetic overflow. 
        /// For example, in the bit to int case, the straightforward calc would 
        /// be (n+31)/32, but that would cause overflow. So instead it's 
        /// rearranged to ((n-1)/32) + 1, with special casing for 0.
        /// 
        /// Usage:
        /// GetArrayLength(77, BitsPerInt32): returns how many ints must be 
        /// allocated to store 77 bits.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="div">use a conversion constant, e.g. BytesPerInt32 to get
        /// how many ints are required to store n bytes</param>
        /// <returns></returns>
        private static int GetArrayLength(int n, int div)
        {
            Contract.Assert(div > 0, "GetArrayLength: div arg must be greater than 0");
            return n > 0 ? (((n - 1) / div) + 1) : 0;
        }

        [Serializable]
        private class BitArrayEnumeratorSimple : IEnumerator, ICloneable
        {
            private BitArrayFast bitarray;
            private int index;
            private int version;
            private bool currentElement;

            internal BitArrayEnumeratorSimple(BitArrayFast bitarray)
            {
                this.bitarray = bitarray;
                this.index = -1;
                version = bitarray._version;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                if (version != bitarray._version) throw new InvalidOperationException("ResId.InvalidOperation_EnumFailedVersion");
                if (index < (bitarray.Count - 1))
                {
                    index++;
                    currentElement = bitarray.GetFast(index);
                    return true;
                }
                else
                    index = bitarray.Count;

                return false;
            }

            public virtual Object Current
            {
                get
                {
                    if (index == -1)
                        throw new InvalidOperationException("ResId.InvalidOperation_EnumNotStarted");
                    if (index >= bitarray.Count)
                        throw new InvalidOperationException("ResId.InvalidOperation_EnumEnded");
                    return currentElement;
                }
            }

            public void Reset()
            {
                if (version != bitarray._version) throw new InvalidOperationException("ResId.InvalidOperation_EnumFailedVersion");
                index = -1;
            }
        }

        internal int[] m_array;
        private int m_length;
        private int _version;
        [NonSerialized]
        private Object _syncRoot;

        private const int _ShrinkThreshold = 256;
    }

}

