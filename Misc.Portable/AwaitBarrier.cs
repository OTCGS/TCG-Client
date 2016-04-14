using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc.Portable
{
    public class AwaitBarrier
    {
        public AwaitBarrier(bool isEnabled)
        {
            this.IsEnabled = isEnabled;
        }

        private bool isEnabled;
        private volatile TaskCompletionSource<object> waiter = new TaskCompletionSource<object>();

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value == isEnabled)
                    return;
                isEnabled = value;
                if (isEnabled)
                {
                    waiter.SetResult(null);
                    waiter = null;
                }
                else
                {
                    waiter = new TaskCompletionSource<object>();
                }
            }
        }

        public Task Barrier
        {
            get
            {
                var waiter = this.waiter;
                if (waiter == null)
                    return Task.FromResult<object>(null);
                return waiter.Task;
            }
        }

    }
}
