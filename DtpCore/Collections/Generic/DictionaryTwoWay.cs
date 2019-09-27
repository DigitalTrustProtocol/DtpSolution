using DtpCore.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DtpCore.Collections.Generic
{
    public class DictionaryTwoWay<T2>
    {
        private static object syncLock = new object();

        
        public Dictionary<T2, int> Index { get; set; } 
        public Dictionary<int, T2> Table { get; set; } 

        public DictionaryTwoWay()
        {
            Table = new Dictionary<int, T2>();
            Index = new Dictionary<T2, int>();
        }

        public DictionaryTwoWay(IEqualityComparer<T2> comparer)
        {
            Table = new Dictionary<int, T2>();
            Index = new Dictionary<T2, int>(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            lock (syncLock)
            {
                return Index.Count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Ensure(T2 value)
        {
            lock (syncLock)
            {
                if (value == null)
                    return 0;

                if (!Index.ContainsKey(value))
                {
                    var index = Index.Count;
                    Index.Add(value, index);
                    Table.Add(index, value);

                    return index;
                }

                return Index[value];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(T2 value)
        {
            lock (syncLock)
            {
                return Index.ContainsKey(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int index)
        {
            lock (syncLock)
            {
                return Table.ContainsKey(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(T2 value)
        {
            lock (syncLock)
            {
                return Index[value];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T2 GetValue(int index)
        {
            lock (syncLock)
            {
                return Table[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int index, out T2 value)
        {
            lock (syncLock)
            {
                return Table.TryGetValue(index, out value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKey(T2 value, out int index)
        {
            lock (syncLock)
            {
                return Index.TryGetValue(value, out index);
            }
        }

    }
}
