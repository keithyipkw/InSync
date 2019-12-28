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
        /// Initializes a <seealso cref="UnlockException"/> with the specified exception thrown before and during releasing the locks and the message.
        /// </summary>
        /// <param name="innerException">The exception thrown before releasing the locks.</param>
        /// <param name="exceptionsDuringUnlock">The exceptions thrown during releasing the locks at the indices in the collection of the locks.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UnlockException(Exception innerException, IDictionary<int, Exception> exceptionsDuringUnlock, string message)
            : base(message, innerException)
        {
            ExceptionsDuringUnlock = new Dictionary<int, Exception>(exceptionsDuringUnlock ?? throw new ArgumentNullException(nameof(exceptionsDuringUnlock)));
        }

        /// <summary>
        /// Initializes a <seealso cref="UnlockException"/> with the specified exception thrown before and during releasing the locks and the message.
        /// </summary>
        /// <param name="exceptionsDuringUnlock">The exceptions thrown during releasing the locks at the indices in the collection of the locks.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UnlockException(IDictionary<int, Exception> exceptionsDuringUnlock, string message)
            : this(null, exceptionsDuringUnlock, message)
        {
        }

        /// <summary>
        /// The exceptions thrown during releasing the locks at the indices in the collection of the locks.
        /// </summary>
        public IReadOnlyDictionary<int, Exception> ExceptionsDuringUnlock { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var description = new StringBuilder();
            description.AppendLine($"{GetType().Name}: {Message}");
            description.AppendLine(string.Join(Environment.NewLine, ExceptionsDuringUnlock.Select(kv => $"    [{kv.Key}]: {kv.Value.GetType().Name}")));

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
