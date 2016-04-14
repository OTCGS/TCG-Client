using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Common
{
    public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : class
    {
        private readonly IDictionary<object, TValue> internalDic = new Dictionary<object, TValue>(new WeekReferenceComparer<TKey>());
        private DateTimeOffset lastPurge;
        private static readonly TimeSpan purgeMinTimeBetween = TimeSpan.FromSeconds(5);

        public TValue this[TKey key]
        {
            get
            {
                PurgeCach();
                try
                {
                    return internalDic[key];
                }
                catch (KeyNotFoundException)
                {
                    return default(TValue);
                }
            }

            set
            {
                PurgeCach();
                internalDic[new WeakReference<TKey>(key)] = value;
            }
        }

        public int Count
        {
            get
            {
                PurgeCach();
                return internalDic.Count;
            }
        }

        private void PurgeCach()
        {

            if (DateTimeOffset.Now - lastPurge > purgeMinTimeBetween)
                ForcePurgeCach();
        }

        public void ForcePurgeCach()
        {
            List<object> toRemove = null;
            foreach (var pair in this.internalDic)
            {
                WeakReference<TKey> weakKey = (WeakReference<TKey>)(pair.Key);
                var weakValue = pair.Value;

                TKey @null;
                if (!weakKey.TryGetTarget(out @null))
                {
                    if (toRemove == null)
                        toRemove = new List<object>();
                    toRemove.Add(weakKey);
                }
            }

            if (toRemove != null)
            {
                foreach (object key in toRemove)
                    this.internalDic.Remove(key);
            }
            lastPurge = DateTimeOffset.Now;
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                PurgeCach();
                return internalDic.Keys.Cast<WeakReference<TKey>>().Select(x =>
                {
                    TKey k;
                    if (x.TryGetTarget(out k))
                        return k;
                    return null;
                }).Where(x => x != null).ToArray();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return internalDic.Values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            PurgeCach();
            internalDic.Add(new KeyValuePair<object, TValue>(new WeakReference<TKey>(item.Key), item.Value));
        }

        public void Add(TKey key, TValue value)
        {
            PurgeCach();
            internalDic.Add(new WeakReference<TKey>(key), value);
        }

        public void Clear()
        {
            internalDic.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            PurgeCach();
            return internalDic.Contains(new KeyValuePair<object, TValue>(item.Key, item.Value));
        }

        public bool ContainsKey(TKey key)
        {
            PurgeCach();
            return internalDic.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            PurgeCach();
            return internalDic.Cast<KeyValuePair<object, TValue>>().Select(x =>
             {
                 var internalKey = x.Key as WeakReference<TKey>;
                 TKey k;
                 if (internalKey.TryGetTarget(out k))
                     return new KeyValuePair<TKey, TValue>?(new KeyValuePair<TKey, TValue>(k, x.Value));
                 return null;
             }).Where(x => x.HasValue).Select(x => x.Value).GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            PurgeCach();
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            PurgeCach();
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            PurgeCach();
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class WeekReferenceComparer<T> : IEqualityComparer<object> where T : class
        {
            public new bool Equals(object x, object y)
            {

                T xVal;
                if (x is WeakReference<T>)
                {
                    var w = x as WeakReference<T>;
                    if (!w.TryGetTarget(out xVal))
                        return false;
                }
                else if (x is T)
                {
                    xVal = x as T;
                }
                else
                    throw new ArgumentException($"Der zu vergleichende Typ muss {typeof(T)} oder WeakReference<{typeof(T)}> sein. x war jedoch {x?.GetType()} und y war {x?.GetType()}.");

                T yVal;
                if (y is WeakReference<T>)
                {
                    var w = y as WeakReference<T>;
                    if (!w.TryGetTarget(out yVal))
                        return false;
                }
                else if (y is T)
                {
                    yVal = y as T;
                }
                else
                    throw new ArgumentException($"Der zu vergleichende Typ muss {typeof(T)} oder WeakReference<{typeof(T)}> sein. x war jedoch {x?.GetType()} und y war {x?.GetType()}.");

                return xVal.Equals(yVal);


            }

            public int GetHashCode(object obj)
            {
                if (obj is WeakReference<T>)
                {
                    var w = obj as WeakReference<T>;
                    T val;
                    if (!w.TryGetTarget(out val))
                    {
                        Logger.Warning($"Objekt nicht länger vorhanden. {typeof(T)}");
                        return -1;
                    }
                    return val.GetHashCode();
                }
                else if (obj is T)
                {
                    var val = obj as T;
                    return val.GetHashCode();
                }
                else
                    throw new ArgumentException($"Der zu vergleichende Typ muss {typeof(T)} oder WeakReference<{typeof(T)}> sein. War jedoch {obj?.GetType()}.");
            }
        }


    }
}
