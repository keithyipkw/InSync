using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Provides abstraction of synchronization. It also allows its protected object to be accessed only after a synchronization begins.
    /// <para>Various asynchronous operations are supported. The protected object is non-null.</para>
    /// </summary>
    public interface IBareAsyncLock
    {
        /// <summary>
        /// Asynchronously acquires the lock and returns the protected non-null object.
        /// </summary>
        /// <returns>The protected non-null object.</returns>
        /// <exception cref="LockException"></exception>
        Task<object> BarelyLockAsync();

        /// <summary>
        /// Asynchronously acquires the lock and returns the protected non-null object.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The protected non-null object.</returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        Task<object> BarelyLockAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(out object value);

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <exception cref="UnlockException"></exception>
        void BarelyUnlock();
    }

    /// <summary>
    /// This interface provides abstraction of synchronization. It also allows its protected object to be accessed only after a synchronization begins.
    /// <para>Various asynchronous operations are supported. The protected object is non-null.</para>
    /// </summary>
    /// <typeparam name="T">The type of the protected object.</typeparam>
    public interface IBareAsyncLock<T> : IBareAsyncLock where T : class
    {
        /// <summary>
        /// Asynchronously acquires the lock and returns the protected non-null value.
        /// </summary>
        /// <returns>The protected non-null value.</returns>
        /// <exception cref="LockException"></exception>
        new Task<T> BarelyLockAsync();

        /// <summary>
        /// Asynchronously acquires the lock and returns the protected non-null value.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The protected non-null value.</returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        new Task<T> BarelyLockAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null value if the lock is acquired.
        /// </summary>
        /// <param name="value">The protected non-null value if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(out T value);
    }
}
