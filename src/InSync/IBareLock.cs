using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    /// <summary>
    /// Provides abstraction of synchronization. It also allows its protected object to be accessed only after a synchronization begins.
    /// <para>Asynchronous operations are not supported. The protected object is non-null.</para>
    /// </summary>
    public interface IBareLock
    {
        /// <summary>
        /// Synchronously acquires the lock and returns the protected non-null object.
        /// </summary>
        /// <returns>The protected non-null object.</returns>
        /// <exception cref="LockException"></exception>
        object BarelyLock();

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(out object value);

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <seealso cref="System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(int millisecondsTimeout, out object value);

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="timeout">A <seealso cref="TimeSpan"/> that represents the number of milliseconds to wait, a <seealso cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <seealso cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(TimeSpan timeout, out object value);

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <exception cref="UnlockException"></exception>
        void BarelyUnlock();
    }

    /// <summary>
    /// Provides abstraction of synchronization. It also allows its protected object to be accessed only after a synchronization begins.
    /// <para>Asynchronous operations are not supported. The protected object is non-null.</para>
    /// </summary>
    /// <typeparam name="T">The type of the protected object.</typeparam>
    public interface IBareLock<T> : IBareLock where T : class
    {
        /// <summary>
        /// Synchronously acquires the lock and returns the protected non-null object.
        /// </summary>
        /// <returns>The protected non-null object.</returns>
        /// <exception cref="LockException"></exception>
        new T BarelyLock();

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(out T value);

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <seealso cref="System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(int millisecondsTimeout, out T value);

        /// <summary>
        /// Tries to acquire the lock. It returns <c>true</c> and the protected non-null object if the lock is acquired.
        /// </summary>
        /// <param name="timeout">A <seealso cref="TimeSpan"/> that represents the number of milliseconds to wait, a <seealso cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <seealso cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="value">The protected non-null object if the lock is acquired, otherwise, <c>null</c> is returned.</param>
        /// <returns><c>true</c> if the lock is acquired.</returns>
        /// <exception cref="LockException"></exception>
        bool BarelyTryLock(TimeSpan timeout, out T value);
    }
}
