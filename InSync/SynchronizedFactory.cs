using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSync
{
    /// <summary>
    /// Provides static methods for creating <seealso cref="Synchronized{T}"/> without explicitly specifying the type of the protected object.
    /// </summary>
    public static class Synchronized
    {
        /// <summary>
        /// Creates a <seealso cref="Synchronized{T}"/> with the object to protect as the lock.
        /// </summary>
        /// <typeparam name="T">The type of the protected object.</typeparam>
        /// <param name="value">The object to protect and the lock</param>
        /// <returns>A new <seealso cref="Synchronized{T}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static Synchronized<T> Create<T>(T value) where T : class
        {
            return new Synchronized<T>(value, value);
        }

        /// <summary>
        /// Creates a new <seealso cref="Synchronized{T}"/> with the specified lock and the object to protect.
        /// </summary>
        /// <typeparam name="T">The type of the protected object.</typeparam>
        /// <param name="padLock">The lock for synchronization</param>
        /// <param name="value">The object to protect.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="padLock"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static Synchronized<T> Create<T>(object padLock, T value) where T : class
        {
            return new Synchronized<T>(padLock, value);
        }
    }
}
