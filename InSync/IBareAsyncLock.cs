using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public interface IBareAsyncLock
    {
        Task<object> BarelyLockAsync();
        Task<object> BarelyLockAsync(CancellationToken cancellationToken);
        bool BarelyTryLock(out object value);
        /// <summary>
        /// This method must not throw exceptions.
        /// </summary>
        void BarelyUnlock();
    }

    public interface IBareAsyncLock<T> : IBareAsyncLock where T : class
    {
        new Task<T> BarelyLockAsync();
        new Task<T> BarelyLockAsync(CancellationToken cancellationToken);
        bool BarelyTryLock(out T value);
    }
}
