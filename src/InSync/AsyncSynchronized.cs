﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        private bool TryAcquire(int millisecondsTimeout)
        {
            try
            {
                return semaphore.Wait(millisecondsTimeout);
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private bool TryAcquire(TimeSpan timeout)
        {
            try
            {
                return semaphore.Wait(timeout);
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
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
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
        public bool BarelyTryLock([NotNullWhen(true)] out T? value)
        {
            if (TryAcquire(0))
            {
                value = this.value;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(int millisecondsTimeout, [NotNullWhen(true)] out T? value)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                value = this.value;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(TimeSpan timeout, [NotNullWhen(true)] out T? value)
        {
            if (TryAcquire(timeout))
            {
                value = this.value;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        bool IBareLock.BarelyTryLock([NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(out T? tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        bool IBareAsyncLock.BarelyTryLock([NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(out T? tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        bool IBareLock.BarelyTryLock(int millisecondsTimeout, [NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(millisecondsTimeout, out T? tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        bool IBareLock.BarelyTryLock(TimeSpan timeout, [NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(timeout, out T? tmp);
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
            await AcquireAsync(cancellationToken).ConfigureAwait(false);
            return value;
        }

        async Task<object> IBareAsyncLock.BarelyLockAsync()
        {
            await AcquireAsync(CancellationToken.None).ConfigureAwait(false);
            return value;
        }

        async Task<object> IBareAsyncLock.BarelyLockAsync(CancellationToken cancellationToken)
        {
            await AcquireAsync(cancellationToken).ConfigureAwait(false);
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
            if (TryAcquire(0))
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
        public bool TryWithLock(int millisecondsTimeout, Action<T> action)
        {
            if (TryAcquire(millisecondsTimeout))
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
        public bool TryWithLock(TimeSpan timeout, Action<T> action)
        {
            if (TryAcquire(timeout))
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
        public GuardedValue<T>? TryLock()
        {
            if (TryAcquire(0))
            {
                return new GuardedValue<T>(value, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public GuardedValue<T>? TryLock(int millisecondsTimeout)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                return new GuardedValue<T>(value, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public GuardedValue<T>? TryLock(TimeSpan timeout)
        {
            if (TryAcquire(timeout))
            {
                return new GuardedValue<T>(value, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public Task WithLockAsync(Action<T> action)
        {
            return WithLockAsync(action, CancellationToken.None, true);
        }

        /// <inheritdoc/>
        public Task WithLockAsync(Action<T> action, bool onCapturedContext)
        {
            return WithLockAsync(action, CancellationToken.None, onCapturedContext);
        }

        /// <inheritdoc/>
        public Task WithLockAsync(Action<T> action, CancellationToken cancellationToken)
        {
            return WithLockAsync(action, cancellationToken, true);
        }

        /// <inheritdoc/>
        public async Task WithLockAsync(Action<T> action, CancellationToken cancellationToken, bool onCapturedContext)
        {
            await AcquireAsync(cancellationToken).ConfigureAwait(onCapturedContext);
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
            return WithLockAsync(func, CancellationToken.None, true);
        }

        /// <inheritdoc/>
        public Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, bool onCapturedContext)
        {
            return WithLockAsync(func, CancellationToken.None, onCapturedContext);
        }

        /// <inheritdoc/>
        public Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken)
        {
            return WithLockAsync(func, cancellationToken, true);
        }

        /// <inheritdoc/>
        public async Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken, bool onCapturedContext)
        {
            await AcquireAsync(cancellationToken).ConfigureAwait(onCapturedContext);
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
            return WithLockAsync(asyncAction, CancellationToken.None, true);
        }

        /// <inheritdoc/>
        public Task WithLockAsync(Func<T, Task> asyncAction, bool onCapturedContext)
        {
            return WithLockAsync(asyncAction, CancellationToken.None, onCapturedContext);
        }
        
        /// <inheritdoc/>
        public Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken)
        {
            return WithLockAsync(asyncAction, cancellationToken, true);
        }

        /// <inheritdoc/>
        public async Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken, bool onCapturedContext)
        {
            await AcquireAsync(cancellationToken).ConfigureAwait(onCapturedContext);
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
            return WithLockAsync(asyncFunc, CancellationToken.None, true);
        }

        /// <inheritdoc/>
        public Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, bool onCapturedContext)
        {
            return WithLockAsync(asyncFunc, CancellationToken.None, onCapturedContext);
        }

        /// <inheritdoc/>
        public Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken)
        {
            return WithLockAsync(asyncFunc, cancellationToken, true);
        }

        /// <inheritdoc/>
        public async Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken, bool onCapturedContext)
        {
            await AcquireAsync(cancellationToken).ConfigureAwait(onCapturedContext);
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
