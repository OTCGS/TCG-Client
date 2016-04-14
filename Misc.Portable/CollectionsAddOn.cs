using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace System.Linq
{
    public static class CollectionsAddOn
    {

        /// <summary>
        /// Ändert die Collection so ab das sie der übergeben entspricht. Dabei werden Elemente die in beiden Parametern vorhanden sind Verschoben anstelle gelöcht und neu Hinzugefügt zu werden.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="data"></param>
        public static void UpdateCollection<T>(this ObservableCollection<T> target, IEnumerable<T> data)
        {
            if (data is IList<T>) // Bei einer liste müssen wir auch die Reihenfolge einhalten.
            {
                var source = data as IList<T>;

                var sourceSet = new HashSet<T>(source);
                var targetSet = new HashSet<T>(target);


                for (int i = target.Count - 1; i >= 0; i--)
                {
                    var current = target[i];
                    if (!sourceSet.Contains(current))
                        target.RemoveAt(i);
                }

                for (int i = 0; i < source.Count; i++)
                {
                    var current = source[i];
                    if (targetSet.Contains(current))
                    {
                        var oldIndex = target.IndexOf(current);
                        if (oldIndex != i)
                            target.Move(oldIndex, i);
                    }
                    else
                        target.Insert(i, current);
                }



            }
            else
            {
                var toAdd = new HashSet<T>(data);
                toAdd.ExceptWith(target);
                var toRemove = new HashSet<T>(target);
                toRemove.ExceptWith(data);
                foreach (var r in toRemove)
                    target.Remove(r);
                foreach (var r in toAdd)
                    target.Add(r);



            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> col, T element)
        {
            foreach (var t in col)
                yield return t;
            yield return element;
        }

        public static int IndexOf<T>(this IEnumerable<T> col, Predicate<T> p)
        {
            var index = 0;
            var e = col.GetEnumerator();
            while (e.MoveNext())
            {
                if (p(e.Current))
                    return index;
                index++;
            }
            return -1;
        }

        public static IEnumerable<TSource> Distinct<TSource, TDistinguish>(this IEnumerable<TSource> source, Func<TSource, TDistinguish> selector)
        {
            return source.Distinct(new DistirctComparator<TSource, TDistinguish>(selector));

        }

        private class DistirctComparator<TSource, TDistinguish> : IEqualityComparer<TSource>
        {
            private Func<TSource, TDistinguish> selector;

            public DistirctComparator(Func<TSource, TDistinguish> selector)
            {
                this.selector = selector;
            }

            public bool Equals(TSource x, TSource y)
            {
                return selector(x).Equals(selector(y));
            }

            public int GetHashCode(TSource obj)
            {
                return selector(obj).GetHashCode();
            }
        }
    }
}