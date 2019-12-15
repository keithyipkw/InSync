using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public class AsyncSynchronized<T> :
        ISynchronized<T>,
        IAsyncSynchronized<T>,
        IBareLock<T>,
        IBareAsyncLock<T>
        where T : class
    {
        public AsyncSynchronized(SemaphoreSlim semaphore, T value)
        {
            this.semaphore = semaphore;
            this.value = value;
        }

        protected readonly SemaphoreSlim semaphore;
        protected readonly T value;

        public T BarelyLock()
        {
            semaphore.Wait();
            return value;
        }

        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        public bool BarelyTryLock(out T value)
        {
            if (semaphore.Wait(0))
            {
                value = this.value;
                return true;
            }
            value = null;
            return false;
        }

        public bool BarelyTryLock(out object value)
        {
            var result = BarelyTryLock(out T tmp);
            value = tmp;
            return result;
        }

        public void BarelyUnlock()
        {
            semaphore.Release();
        }
        
        public Task<T> BarelyLockAsync()
        {
            return BarelyLockAsync(CancellationToken.None);
        }

        public async Task<T> BarelyLockAsync(CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            return value;
        }

        Task<object> IBareAsyncLock.BarelyLockAsync()
        {
            return IBareAsyncLockBarelyLockAsync(CancellationToken.None);
        }

        Task<object> IBareAsyncLock.BarelyLockAsync(CancellationToken cancellationToken)
        {
            return IBareAsyncLockBarelyLockAsync(cancellationToken);
        }

        private async Task<object> IBareAsyncLockBarelyLockAsync(CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            return value;
        }

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

        public TResult WithLock<TResult>(Func<T, TResult> func)
        {
            semaphore.Wait();
            try
            {
                return func(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public GuardedValue<T> Lock()
        {
            semaphore.Wait();
            return new GuardedValue<T>(value, BarelyUnlock);
        }

        public bool TryWithLock(Action<T> action)
        {
            if (semaphore.Wait(0))
            {
                try
                {
                    action(value);
                    return true;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            return false;
        }

        public bool TryWithLock<TResult>(Func<T, TResult> func, out TResult result)
        {
            if (semaphore.Wait(0))
            {
                try
                {
                    result = func(value);
                    return true;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            result = default;
            return false;
        }

        public GuardedValue<T> TryLock()
        {
            if (semaphore.Wait(0))
            {
                return new GuardedValue<T>(value, BarelyUnlock);
            }
            return null;
        }

        public Task WithLockAsync(Action<T> action)
        {
            return WithLockAsync(action, CancellationToken.None);
        }

        public async Task WithLockAsync(Action<T> action, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                action(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func)
        {
            return WithLockAsync(func, CancellationToken.None);
        }

        public async Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return func(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public Task WithLockAsync(Func<T, Task> asyncAction)
        {
            return WithLockAsync(asyncAction, CancellationToken.None);
        }

        public async Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await asyncAction(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc)
        {
            return WithLockAsync(asyncFunc, CancellationToken.None);
        }

        public async Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await asyncFunc(value);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public Task<GuardedValue<T>> LockAsync()
        {
            return LockAsync(CancellationToken.None);
        }

        public async Task<GuardedValue<T>> LockAsync(CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            return new GuardedValue<T>(value, BarelyUnlock);
        }
    }
}
