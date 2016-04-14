using System;
using System.Collections.Generic;

//using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    public class ConcurrentChannel<T> : IDisposable
    {
        private readonly Queue<TaskCompletionSource<T>> waitingQueue = new Queue<TaskCompletionSource<T>>();
        private readonly Queue<TaskCompletionSource<T>> completedQueue = new Queue<TaskCompletionSource<T>>();

        private readonly System.Threading.SemaphoreSlim semaphore = new Threading.SemaphoreSlim(1, 1);

        public async Task Send(T t)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.ToString());
            await semaphore.WaitAsync();
            if (waitingQueue.Count == 0)
            {
                var tcs = new TaskCompletionSource<T>();
                tcs.SetResult(t);
                completedQueue.Enqueue(tcs);
            }
            else
            {
                var tcs = waitingQueue.Dequeue();
                tcs.SetResult(t);
            }
            semaphore.Release();
        }

        public async Task<T> Recive()
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.ToString());
            await semaphore.WaitAsync();

            if (completedQueue.Count == 0)
            {
                var t = new TaskCompletionSource<T>();
                waitingQueue.Enqueue(t);
                semaphore.Release();
                return await t.Task;
            }
            else
            {
                var t = completedQueue.Dequeue();
                semaphore.Release();
                return await t.Task;
            }
        }

        /// <summary>
        /// Gibt das Objekt zurück, welches als nächstes Zurückgegeben werden würde.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Wurde vorher Recive aufgerufen, welches noch erwartet wird, so liefert
        /// Peek nicht das noch fehlende Element, sondern das element welches nach
        /// dem bereits angefordertem kommt.
        /// </remarks>
        public async Task<T> Peek()
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.ToString());
            await semaphore.WaitAsync();

            if (completedQueue.Count == 0)
            {
                var t = new TaskCompletionSource<T>();
                waitingQueue.Enqueue(t);
                completedQueue.Enqueue(t);
                semaphore.Release();
                return await t.Task;
            }
            else
            {
                var t = completedQueue.Peek();
                semaphore.Release();
                return await t.Task;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    semaphore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        // ~ConcurrentChannel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}