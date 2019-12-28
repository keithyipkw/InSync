using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Represents the reader part of <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/>.
    /// </summary>
    /// <typeparam name="TRead">The type of the reader to protect.</typeparam>
    public class ReaderSynchronized<TRead> : ISynchronized<TRead>, IBareLock<TRead>
        where TRead : class
    {
        /// <summary>
        /// Initialize a <seealso cref="ReaderSynchronized{TRead}"/> with the specified <seealso cref="readerWriterLockSlim"/> and reader.
        /// </summary>
        /// <param name="readerWriterLockSlim"></param>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException"><paramref name="readerWriterLockSlim"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public ReaderSynchronized(ReaderWriterLockSlim readerWriterLockSlim, TRead reader)
        {
            this.readerWriterLockSlim = readerWriterLockSlim ?? throw new ArgumentNullException(nameof(readerWriterLockSlim));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        /// The <seealso cref="ReaderWriterLockSlim"/> for synchronization.
        /// </summary>
        protected readonly ReaderWriterLockSlim readerWriterLockSlim;

        /// <summary>
        /// The reader to protect.
        /// </summary>
        protected readonly TRead reader;

        /// <inheritdoc/>
        public TRead BarelyLock()
        {
            readerWriterLockSlim.EnterReadLock();
            return reader;
        }

        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(out TRead value)
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                value = this.reader;
                return true;
            }
            value = null;
            return false;
        }
        
        bool IBareLock.BarelyTryLock(out object value)
        {
            var result = BarelyTryLock(out var tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        public void BarelyUnlock()
        {
            readerWriterLockSlim.ExitReadLock();
        }

        /// <inheritdoc/>
        public void WithLock(Action<TRead> action)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                action(reader);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public TResult WithLock<TResult>(Func<TRead, TResult> func)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                return func(reader);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public GuardedValue<TRead> Lock()
        {
            readerWriterLockSlim.EnterReadLock();
            return new GuardedValue<TRead>(reader, BarelyUnlock);
        }

        /// <inheritdoc/>
        public bool TryWithLock(Action<TRead> action)
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                try
                {
                    action(reader);
                    return true;
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryWithLock<TResult>(Func<TRead, TResult> func, out TResult result)
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                try
                {
                    result = func(reader);
                    return true;
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
            result = default;
            return false;
        }

        /// <inheritdoc/>
        public GuardedValue<TRead> TryLock()
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                return new GuardedValue<TRead>(reader, BarelyUnlock);
            }
            return null;
        }
    }
}
