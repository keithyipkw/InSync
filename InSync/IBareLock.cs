using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    public interface IBareLock
    {
        /// <summary>
        /// Synchronously acquires the lock and returns the protected non-null value.
        /// </summary>
        /// <returns>The protected non-null value</returns>
        object BarelyLock();

        /// <summary>
        /// Attempts to acquire the lock. <code>true</code> and the protected non-null value are returned if the lock is acquired.
        /// </summary>
        /// <param name="value">The protected non-null value</param>
        /// <returns><code>true</code> if the lock is acquired, otherwise <code>false</code>.</returns>
        bool BarelyTryLock(out object value);

        /// <summary>
        /// Unlocks the lock. No exceptions should be thrown.
        /// </summary>
        void BarelyUnlock();
    }

    public interface IBareLock<T> : IBareLock where T : class
    {
        /// <summary>
        /// Synchronously acquires the lock and returns the protected non-null value.
        /// </summary>
        /// <returns>The protected non-null value</returns>
        new T BarelyLock();

        /// <summary>
        /// Attempts to acquire the lock. <code>true</code> and the protected non-null value are returned if the lock is acquired.
        /// </summary>
        /// <param name="value">The protected non-null value</param>
        /// <returns><code>true</code> if the lock is acquired, otherwise <code>false</code>.</returns>
        bool BarelyTryLock(out T value);
    }
}
