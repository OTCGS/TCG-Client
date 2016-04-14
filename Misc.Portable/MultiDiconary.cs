using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public class MultiDiconary<TKey1, TKey2, TValue> : Dictionary<Tuple<TKey1, TKey2>, TValue>
    {
        public TValue this[TKey1 k1, TKey2 k2]
        {
            get
            {
                try
                {
                    return this[Tuple.Create(k1, k2)];
                }
                catch (Exception)
                {

                    throw;
                } }
            set { this[Tuple.Create(k1, k2)] = value; }
        }

        public void Add(TKey1 k1, TKey2 k2, TValue value)
        {
            this.Add(Tuple.Create(k1, k2), value);
        }

        public bool ContainsKey(TKey1 k1, TKey2 k2)
        {
            return ContainsKey(Tuple.Create(k1, k2));
        }

        public bool Remove(TKey1 k1, TKey2 k2)
        {
            return Remove(Tuple.Create(k1, k2));
        }

        public bool TryGetValue(TKey1 k1, TKey2 k2, out TValue value)
        {
            return TryGetValue(Tuple.Create(k1, k2), out value);
        }
    }
}