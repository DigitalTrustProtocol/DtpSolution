using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Collections.Generic
{
    public class DictionaryTwoWay<T2>
    {
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

        public int Count()
        {
            return _forward.Count;
        }

        public int Ensure(T2 value)
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

        public bool ContainsKey(T2 value)
        {
            return _forward.ContainsKey(value);
        }

        public bool ContainsKey(int index)
        {
            return _reverse.ContainsKey(index);
        }

        public int GetIndex(T2 value)
        {
            return _forward[value];
        }

        public T2 GetValue(int index)
        {
            return _reverse[index];
        }
        public bool TryGetValue(int index, out T2 value)
        {
            return _reverse.TryGetValue(index, out value);
        }
    }
}
