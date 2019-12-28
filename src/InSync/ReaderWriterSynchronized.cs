using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Uses <seealso cref="ReaderWriterLockSlim"/> to synchronize and only expose its protected reader or writer after a respective read or write synchronization begins. All the methods implementing its interfaces refer to the writer part. The readers are accessible via <seealso cref="ReaderWriterSynchronized{TWrite, TRead}.Reader"/> and <seealso cref="ReaderWriterSynchronized{TWrite, TRead}.UpgradeableReader"/>.
    /// <para>Asynchronous operations are not supported. The protected reader and writer are non-null.</para>
    /// </summary>
    /// <typeparam name="TWrite">The type of the writer to protect.</typeparam>
    /// <typeparam name="TRead">The type of the reader to protect.</typeparam>
    public class ReaderWriterSynchronized<TWrite, TRead> : ISynchronized<TWrite>, IBareLock<TWrite>
        where TWrite : class
        where TRead : class
    {
        /// <summary>
        /// Initializes a <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/> with the specified <seealso cref="ReaderWriterLockSlim"/>, writer to protect and reader to protect.
        /// </summary>
        /// <param name="readerWriterLockSlim">The <seealso cref="ReaderWriterLockSlim"/> for synchronization.</param>
        /// <param name="writer">The writer to protect.</param>
        /// <param name="reader">The reader to protect.</param>
        /// <exception cref="ArgumentNullException"><paramref name="readerWriterLockSlim"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public ReaderWriterSynchronized(ReaderWriterLockSlim readerWriterLockSlim, TWrite writer, TRead reader)
        {
            this.readerWriterLockSlim = readerWriterLockSlim ?? throw new ArgumentNullException(nameof(readerWriterLockSlim));
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            Reader = new ReaderSynchronized<TRead>(readerWriterLockSlim, reader);
            UpgradeableReader = new UpgradeableReaderSynchronized<TRead>(readerWriterLockSlim, reader);
        }

        /// <summary>
        /// The <seealso cref="ReaderWriterLockSlim"/> for synchronization.
        /// </summary>
        protected readonly ReaderWriterLockSlim readerWriterLockSlim;

        /// <summary>
        /// The writer to protect.
        /// </summary>
        protected readonly TWrite writer;

        /// <summary>
        /// The reader to protect.
        /// </summary>
        protected readonly TRead reader;

        /// <summary>
        /// Gets the synchronized reader.
        /// </summary>
        public ReaderSynchronized<TRead> Reader { get; }

        /// <summary>
        /// Gets the synchronized upgradeable reader.
        /// </summary>
        public UpgradeableReaderSynchronized<TRead> UpgradeableReader { get; }

        /// <inheritdoc/>
        public TWrite BarelyLock()
        {
            readerWriterLockSlim.EnterWriteLock();
            return writer;
        }
        
        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(out TWrite value)
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                value = this.writer;
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

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        /// <exception cref="UnlockException"></exception>
        public void BarelyUnlock()
        {
            readerWriterLockSlim.ExitWriteLock();
        }

        /// <inheritdoc/>
        public void WithLock(Action<TWrite> action)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                action(writer);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public TResult WithLock<TResult>(Func<TWrite, TResult> func)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                return func(writer);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public GuardedValue<TWrite> Lock()
        {
            readerWriterLockSlim.EnterWriteLock();
            return new GuardedValue<TWrite>(writer, BarelyUnlock);
        }

        /// <inheritdoc/>
        public bool TryWithLock(Action<TWrite> action)
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                try
                {
                    action(writer);
                    return true;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryWithLock<TResult>(Func<TWrite, TResult> func, out TResult result)
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                try
                {
                    result = func(writer);
                    return true;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
            result = default;
            return false;
        }

        /// <inheritdoc/>
        public GuardedValue<TWrite> TryLock()
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                return new GuardedValue<TWrite>(writer, BarelyUnlock);
            }
            return null;
        }
    }

    /// <summary>
    /// Provides a simpler version of <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/> where the reader and writer are the same object.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    public class ReaderWriterSynchronized<T> : ReaderWriterSynchronized<T, T>
        where T : class
    {
        /// <summary>
        /// Initializes a <seealso cref="ReaderWriterSynchronized{T}"/> with the specified object to protect.
        /// </summary>
        /// <param name="readerWriterLockSlim">The <seealso cref="ReaderWriterLockSlim"/> for synchronization.</param>
        /// <param name="value">The object to protect.</param>
        /// <exception cref="ArgumentNullException"><paramref name="readerWriterLockSlim"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ReaderWriterSynchronized(ReaderWriterLockSlim readerWriterLockSlim, T value) : base(readerWriterLockSlim, value, value)
        {
        }
    }
}
