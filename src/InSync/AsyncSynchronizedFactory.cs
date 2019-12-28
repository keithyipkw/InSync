using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSync
{
    /// <summary>
    /// Provides static methods for creating <seealso cref="AsyncSynchronized{T}"/> without explicitly specifying the type of the protected object.
    /// </summary>
    public static class AsyncSynchronized
    {
        /// <summary>
        /// Creates a <seealso cref="AsyncSynchronized{T}"/> with the a new <seealso cref="SemaphoreSlim"/> which its initial count is 1 and the specified object to protect.
        /// </summary>
        /// <typeparam name="T">The type of the protected object.</typeparam>
        /// <param name="value">The object to protect.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static AsyncSynchronized<T> Create<T>(T value) where T : class
        {
            return new AsyncSynchronized<T>(new SemaphoreSlim(1), value);
        }

        /// <summary>
        /// Creates a <seealso cref="AsyncSynchronized{T}"/> with the specified <seealso cref="SemaphoreSlim"/> and object to protect.
        /// </summary>
        /// <typeparam name="T">The type of the protected object.</typeparam>
        /// <param name="semaphore">The semaphore for synchronization.</param>
        /// <param name="value">The object to protect.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static AsyncSynchronized<T> Create<T>(SemaphoreSlim semaphore, T value) where T : class
        {
            return new AsyncSynchronized<T>(semaphore, value);
        }
    }
}
