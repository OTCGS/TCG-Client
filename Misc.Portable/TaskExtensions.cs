using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading.Tasks
{
    public class TaskEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pre"></param>
        /// <param name="t"></param>
        /// <returns>The finished Task, or <code>Task.FromResult(default(T))</code></returns>
        public static Task<Task<T>> WhenAny<T>(Predicate<T> pre, params Task<T>[] t)
        {
            return WhenAny(pre, t as IEnumerable<Task<T>>);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pre"></param>
        /// <param name="t"></param>
        /// <returns>The finished Task, or <code>Task.FromResult(default(T))</code></returns>
        public async static Task<Task<T>> WhenAny<T>(Predicate<T> pre, IEnumerable<Task<T>> t)
        {
            Task<T> erg = null;
            HashSet<Task<T>> list = null;
            do
            {
                IEnumerable<Task<T>> en;
                if (list == null)
                    en = t;
                else
                    en = list;
            
                var result = await Task.WhenAny(en);
                if (pre(await result))
                    erg = result;
                if (list == null)
                    list = new HashSet<Task<T>>(t);
                list.Remove(result);

            } while (erg == null && list.Any());

            return erg ?? Task.FromResult(default(T));
        }

    }
}
