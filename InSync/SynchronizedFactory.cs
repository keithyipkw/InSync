using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSync
{
    public static class Synchronized
    {
        /// <summary>
        /// Create a <seealso cref="Synchronized{T}"/> and use the value as the lock.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Synchronized<T> Create<T>(T value) where T : class
        {
            return new Synchronized<T>(value, value);
        }

        public static Synchronized<T> Create<T>(object padLock, T value) where T : class
        {
            return new Synchronized<T>(padLock, value);
        }
    }
}
