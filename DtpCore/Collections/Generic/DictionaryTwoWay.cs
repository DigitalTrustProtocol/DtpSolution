using DtpCore.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DtpCore.Collections.Generic
{
    public class DictionaryTwoWay<T2>
    {
        private static object syncLock = new object();


        private readonly Dictionary<T2, int> _forward = new Dictionary<T2, int>();
        private readonly Dictionary<int, T2> _reverse = new Dictionary<int, T2>();

        public DictionaryTwoWay()
        {
            _reverse = new Dictionary<int, T2>();
            _forward = new Dictionary<T2, int>();
        }

        public DictionaryTwoWay(IEqualityComparer<T2> comparer)
        {
            _reverse = new Dictionary<int, T2>();
            _forward = new Dictionary<T2, int>(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            lock (syncLock)
            {
                return _forward.Count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Ensure(T2 value)
        {
            lock (syncLock)
            {
                if (value == null)
                    return 0;

                if (!_forward.ContainsKey(value))
                {
                    var index = Count();
                    _forward.Add(value, index);
                    _reverse.Add(index, value);

                    return index;
                }

                return _forward[value];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(T2 value)
        {
            lock (syncLock)
            {
                return _forward.ContainsKey(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int index)
        {
            lock (syncLock)
            {
                return _reverse.ContainsKey(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(T2 value)
        {
            lock (syncLock)
            {
                return _forward[value];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T2 GetValue(int index)
        {
            lock (syncLock)
            {
                return _reverse[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int index, out T2 value)
        {
            lock (syncLock)
            {
                return _reverse.TryGetValue(index, out value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKey(T2 value, out int index)
        {
            lock (syncLock)
            {
                return _forward.TryGetValue(value, out index);
            }
        }

    }
}
