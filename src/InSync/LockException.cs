using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    /// <summary>
    /// The exception that is thrown when an exception is thrown when acquiring a lock.
    /// </summary>
    public class LockException : Exception
    {
        /// <summary>
        /// Initializes a <seealso cref="LockException"/> with the specified causing exception.
        /// </summary>
        /// <param name="innerException">The original exception thrown when acquiring a lock.</param>
        /// <exception cref="ArgumentNullException"><paramref name="innerException"/> is <c>null</c>.</exception>
        public LockException(Exception innerException)
            : base("An exception is thrown when acquiring the lock.",
                  innerException ?? throw new ArgumentNullException(nameof(innerException)))
        {
        }
    }
}
