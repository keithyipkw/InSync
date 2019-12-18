using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    public class UnlockException : Exception
    {
        public UnlockException(Exception innerException, IEnumerable<Exception> exceptionsFromUnlocking, string message)
            : base(message, innerException)
        {
            ExceptionsFromUnlocking = exceptionsFromUnlocking != null ? new List<Exception>(exceptionsFromUnlocking) : throw new ArgumentNullException(nameof(exceptionsFromUnlocking));
        }

        public UnlockException(IEnumerable<Exception> innerExceptions, string message)
            : this(null, innerExceptions, message)
        {
        }
        
        public IReadOnlyCollection<Exception> ExceptionsFromUnlocking { get; }
    }
}
