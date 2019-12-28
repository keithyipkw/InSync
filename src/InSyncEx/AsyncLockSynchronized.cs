using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InSync.AsyncEx
{
    public class AsyncLockSynchronized<T> where T : class
    {
        public AsyncLockSynchronized(AsyncLock padLock, T value)
        {
            this.padLock = padLock;
            this.value = value;
        }

        protected readonly AsyncLock padLock;
        protected readonly T value;

        public void WithLock(Action<T> action)
        {
            using (padLock.Lock())
            {
                action(value);
            }
        }

        public async Task WithLock(Func<T, Task> asyncAction)
        {
            using (padLock.Lock())
            {
                await asyncAction(value);
            }
        }

        public async Task WithLockAsync(Action<T> action)
        {
            using (await padLock.LockAsync())
            {
                action(value);
            }
        }

        public async Task WithLockAsync(Func<T, Task> asyncAction)
        {
            using (await padLock.LockAsync())
            {
                await asyncAction(value);
            }
        }
    }
}
