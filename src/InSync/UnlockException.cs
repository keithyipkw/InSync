using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InSync
{
    /// <summary>
    /// The exception that is thrown when an exception is thrown releasing some locks. There may be another exception thrown before releasing the locks. It is stored in <seealso cref="Exception.InnerException"/>.
    /// </summary>
    public class UnlockException : Exception
    {
        /// <summary>
        /// Initializes a <seealso cref="UnlockException"/> with the specified exceptions thrown before and during releasing the locks.
        /// </summary>
        /// <param name="priorException">The exception thrown before releasing the locks.</param>
        /// <param name="exceptionsDuringUnlock">The exceptions thrown during releasing the locks at the indices in the collection of the locks.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exceptionsDuringUnlock"/> is <c>null</c>.</exception>
        public UnlockException(Exception priorException, IDictionary<int, Exception> exceptionsDuringUnlock)
            : this(priorException, exceptionsDuringUnlock, "An exception occurred then other exceptions occurred when releasing locks")
        {
        }

        /// <summary>
        /// Initializes a <seealso cref="UnlockException"/> with the specified exceptions thrown before and during releasing the locks.
        /// </summary>
        /// <param name="exceptionsDuringUnlock">The exceptions thrown during releasing the locks at the indices in the collection of the locks.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exceptionsDuringUnlock"/> is <c>null</c>.</exception>
        public UnlockException(IDictionary<int, Exception> exceptionsDuringUnlock)
            : this(null, exceptionsDuringUnlock, "Exceptions occurred when releasing locks")
        {
        }

        /// <summary>
        /// Initializes a <seealso cref="UnlockException"/> with the specified exception thrown during releasing the lock.
        /// </summary>
        /// <param name="priorException">The exception thrown before releasing the lock.</param>
        /// <param name="exceptionDuringUnlock">The exception thrown when releasing the lock.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exceptionDuringUnlock"/> is <c>null</c>.</exception>
        public UnlockException(Exception priorException, Exception exceptionDuringUnlock)
            : this(priorException, exceptionDuringUnlock, "An exception occurred then another exception occurred when releasing locks")
        {
        }

        /// <summary>
        /// Initializes a <seealso cref="UnlockException"/> with the specified exception thrown during releasing the lock.
        /// </summary>
        /// <param name="exceptionDuringUnlock">The exception thrown when releasing the lock.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exceptionDuringUnlock"/> is <c>null</c>.</exception>
        public UnlockException(Exception exceptionDuringUnlock)
            : this(null, exceptionDuringUnlock, "An exception occurred when releasing locks")
        {
        }

        private UnlockException(Exception priorException, IDictionary<int, Exception> exceptionsDuringUnlock, string message)
            : base(message,
                  (exceptionsDuringUnlock ?? throw new ArgumentNullException(nameof(exceptionsDuringUnlock)))
                  .OrderBy(kv => kv.Key)
                  .First().Value)
        {
            PriorException = priorException;
            InnerExceptions = new Dictionary<int, Exception>(exceptionsDuringUnlock);
        }

        private UnlockException(Exception priorException, Exception exceptionDurationUnlock, string message)
            : base(message, exceptionDurationUnlock)
        {
            PriorException = priorException;
            InnerExceptions = new Dictionary<int, Exception>
            {
                [0] = exceptionDurationUnlock ?? throw new ArgumentNullException(nameof(exceptionDurationUnlock))
            };
        }

        /// <summary>
        /// The exception thrown before releasing the locks.
        /// </summary>
        public Exception PriorException { get; }

        /// <summary>
        /// The exceptions thrown during releasing the locks at the indices in the collection of the locks.
        /// </summary>
        public IReadOnlyDictionary<int, Exception> InnerExceptions { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var description = new StringBuilder();
            description.AppendLine($"{GetType().Name}: {Message}");
            description.AppendLine(string.Join(Environment.NewLine, InnerExceptions.Select(kv => $"    [{kv.Key}]: {kv.Value.GetType().Name}")));

            if (InnerException != null)
            {
                description.AppendLine($" ---> {InnerException}");
                description.AppendLine("   --- End of inner exception stack trace ---");
            }
            description.Append(StackTrace);

            return description.ToString();
        }
    }
}
