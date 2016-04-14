using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    public class AwaitableConcurrentQueue<T>
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> backingQueue = new System.Collections.Concurrent.ConcurrentQueue<T>();
        private TaskCompletionSource<object> waiter = new TaskCompletionSource<object>();

        private object guard = new object();

        public int Count => backingQueue.Count;

        public void Enqueue(T element)
        {
            backingQueue.Enqueue(element);
            lock (guard)
            {
                waiter.TrySetResult(null);
            }
        }

        public async Task<T> DeQueue()
        {
            T result = default(T);
            bool succes = false;
            do
            {
                if (backingQueue.IsEmpty)
                {
                    lock (guard)
                    {
                        if (backingQueue.IsEmpty)// Falls sie zwichen dem letzten IsEmpty und dem Lock nicht mehr leer ist könnte der waiter bereits gesetzt sein. Daher noch mal Prüfen
                            waiter = new TaskCompletionSource<object>();
                    }
                    await waiter.Task;
                }
                else
                    succes = backingQueue.TryDequeue(out result);
            } while (!succes);
            return result;
        }
    }
}