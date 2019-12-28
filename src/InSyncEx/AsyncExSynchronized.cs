using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InSync.AsyncEx
{
    public class AsyncExSynchronized<T> where T : class
    {
        public AsyncExSynchronized(AsyncSemaphore semaphore, T value)
        {
            this.semaphore = semaphore;
            this.value = value;
        }

        protected readonly AsyncSemaphore semaphore;
        protected readonly T value;

        public void WithLock(Action<T> action)
        {
            semaphore.Wait();
            try
            {
                action(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task WithLock(Func<T, Task> asyncAction)
        {
            semaphore.Wait();
            try
            {
                await asyncAction(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task WithLockAsync(Action<T> action)
        {
            await semaphore.WaitAsync();
            try
            {
                action(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task WithLockAsync(Func<T, Task> asyncAction)
        {
            await semaphore.WaitAsync();
            try
            {
                await asyncAction(value);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
