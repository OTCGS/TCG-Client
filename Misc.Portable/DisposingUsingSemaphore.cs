using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading
{
    public  class DisposingUsingSemaphore :IDisposable
    {
        private System.Threading.SemaphoreSlim EventSemaphore { get; } = new System.Threading.SemaphoreSlim(1, 1);

        ~DisposingUsingSemaphore()
        {
            Dispose(false);
        }
        bool isDisposed = false;
        private void Dispose(bool v)
        {
            if (isDisposed)
                throw new ObjectDisposedException(this.ToString());
            isDisposed = true;
            if (v)
            {
                GC.SuppressFinalize(this);
            }
            EventSemaphore.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task<IDisposable> Enter()
        {
            await EventSemaphore.WaitAsync();
            
            return new Disposer(()=> EventSemaphore.Release());
        }


        private class Disposer : IDisposable
        {
            private Action p;

            public Disposer(Action p)
            {
                this.p = p;
            }

            public void Dispose()
            {
                p();
            }
        }

    }
}
