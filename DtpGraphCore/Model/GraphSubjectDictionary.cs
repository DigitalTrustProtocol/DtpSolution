using System;
using System.Collections.Generic;
using System.Text;

namespace DtpGraphCore.Model
{
    public class GraphSubjectDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {

        public GraphSubjectDictionary()
        {

        }
        public GraphSubjectDictionary(int size) : base(size)
        {

        }
    }
}
