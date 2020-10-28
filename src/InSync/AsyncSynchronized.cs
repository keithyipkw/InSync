using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Uses <seealso cref="SemaphoreSlim"/> to synchronize and only expose its protected object after a synchronization begins.
    /// <para>Various asynchronous operations are supported. Reentrant is not supported. The protected object is non-null.</para>
    /// </summary>
    /// <typeparam name="T">The type of the protected object.</typeparam>
    public class AsyncSynchronized<T> :
        ISynchronized<T>,
        IAsyncSynchronized<T>,
        IBareLock<T>,
        IBareAsyncLock<T>
        where T : class
    {
        /// <summary>
        /// Initializes a <seealso cref="AsyncSynchronized{T}"/> with the specified <seealso cref="SemaphoreSlim"/> and object to protect.
        /// </summary>
        /// <param name="semaphore">The semaphore for synchronization.</param>
        /// <param name="value">The object to protect.</param>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public AsyncSynchronized(SemaphoreSlim semaphore, T value)
        {
            this.semaphore = semaphore;
            this.value = value;
        }

        /// <summary>
        /// The semaphore for synchronization.
        /// </summary>
        protected readonly SemaphoreSlim semaphore;

        /// <summary>
        /// The non-null object to protect.
        /// </summary>
        protected readonly T value;

        private void Acquire()
        {
            try
            {
                semaphore.Wait();
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private bool TryAcquire()
        {
            try
            {
                return semaphore.Wait(0);
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private async Task AcquireAsync(CancellationToken cancellationToken)
        {
            try
            {
                await semaphore.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private void Release(Exception priorException)
        {
            try
            {
                semaphore.Release();
            }
            catch (Exception e)
            {
                throw new UnlockException(priorException, e);
            }
        }

        /// <inheritdoc/>
        public T BarelyLock()
        {
            Acquire();
            return value;
        }

        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(out T value)
        {
            if (TryAcquire())
            {
                value = this.value;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(out object value)
        {
            var result = BarelyTryLock(out T tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        public void BarelyUnlock()
        {
            try
            {
                semaphore.Release();
            }
            catch (Exception e)
            {
                throw new UnlockException(e);
            }
        }
        
        /// <inheritdoc/>
        public Task<T> BarelyLockAsync()
        {
            return BarelyLockAsync(CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<T> BarelyLockAsync(CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            return value;
        }

        async Task<object> IBareAsyncLock.BarelyLockAsync()
        {
            await AcquireAsync(CancellationToken.None);
            return value;
        }

        async Task<object> IBareAsyncLock.BarelyLockAsync(CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            return value;
        }

        /// <inheritdoc/>
        public void WithLock(Action<T> action)
        {
            Acquire();
            try
            {
                action(value);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
        }
        
        /// <inheritdoc/>
        public TResult WithLock<TResult>(Func<T, TResult> func)
        {
            Acquire();
            TResult result;
            try
            {
                result = func(value);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
            return result;
        }

        /// <inheritdoc/>
        public GuardedValue<T> Lock()
        {
            Acquire();
            return new GuardedValue<T>(value, BarelyUnlock);
        }

        /// <inheritdoc/>
        public bool TryWithLock(Action<T> action)
        {
            if (TryAcquire())
            {
                try
                {
                    action(value);
                }
                catch (Exception e)
                {
                    Release(e);
                    throw;
                }
                BarelyUnlock();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryWithLock<TResult>(Func<T, TResult> func, out TResult result)
        {
            if (TryAcquire())
            {
                try
                {
                    result = func(value);
                }
                catch (Exception e)
                {
                    Release(e);
                    throw;
                }
                BarelyUnlock();
                return true;
            }
            result = default;
            return false;
        }

        /// <inheritdoc/>
        public GuardedValue<T> TryLock()
        {
            if (TryAcquire())
            {
                return new GuardedValue<T>(value, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public Task WithLockAsync(Action<T> action)
        {
            return WithLockAsync(action, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task WithLockAsync(Action<T> action, CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            try
            {
                action(value);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
        }

        /// <inheritdoc/>
        public Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func)
        {
            return WithLockAsync(func, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            TResult result;
            try
            {
                result = func(value);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
            return result;
        }

        /// <inheritdoc/>
        public Task WithLockAsync(Func<T, Task> asyncAction)
        {
            return WithLockAsync(asyncAction, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            try
            {
                await asyncAction(value);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
        }

        /// <inheritdoc/>
        public Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc)
        {
            return WithLockAsync(asyncFunc, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            TResult result;
            try
            {
                result = await asyncFunc(value);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
            return result;
        }

        /// <inheritdoc/>
        public Task<GuardedValue<T>> LockAsync()
        {
            return LockAsync(CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<GuardedValue<T>> LockAsync(CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken);
            return new GuardedValue<T>(value, BarelyUnlock);
        }
    }
}
