using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        
        private void Acquire()
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private bool TryAcquire(int millisecondsTimeout)
        {
            try
            {
                return readerWriterLockSlim.TryEnterWriteLock(millisecondsTimeout);
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private bool TryAcquire(TimeSpan timeout)
        {
            try
            {
                return readerWriterLockSlim.TryEnterWriteLock(timeout);
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private void Release(Exception priorException)
        {
            try
            {
                readerWriterLockSlim.ExitWriteLock();
            }
            catch (Exception e)
            {
                throw new UnlockException(priorException, e);
            }
        }

        /// <inheritdoc/>
        public TWrite BarelyLock()
        {
            Acquire();
            return writer;
        }
        
        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        /// <inheritdoc/>
        public bool BarelyTryLock([NotNullWhen(true)] out TWrite? value)
        {
            if (TryAcquire(0))
            {
                value = this.writer;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(int millisecondsTimeout, [NotNullWhen(true)] out TWrite? value)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                value = this.writer;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(TimeSpan timeout, [NotNullWhen(true)] out TWrite? value)
        {
            if (TryAcquire(timeout))
            {
                value = this.writer;
                return true;
            }
            value = null;
            return false;
        }

        bool IBareLock.BarelyTryLock([NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(out var tmp);
            value = tmp;
            return result;
        }

        bool IBareLock.BarelyTryLock(int millisecondsTimeout, [NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(millisecondsTimeout, out var tmp);
            value = tmp;
            return result;
        }

        bool IBareLock.BarelyTryLock(TimeSpan timeout, [NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(timeout, out var tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        public void BarelyUnlock()
        {
            try
            {
                readerWriterLockSlim.ExitWriteLock();
            }
            catch (Exception e)
            {
                throw new UnlockException(e);
            }
        }
        
        /// <inheritdoc/>
        public void WithLock(Action<TWrite> action)
        {
            Acquire();
            try
            {
                action(writer);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
        }

        /// <inheritdoc/>
        public TResult WithLock<TResult>(Func<TWrite, TResult> func)
        {
            Acquire();
            TResult result;
            try
            {
                result = func(writer);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
            return result;
        }

        /// <inheritdoc/>
        public GuardedValue<TWrite> Lock()
        {
            Acquire();
            return new GuardedValue<TWrite>(writer, BarelyUnlock);
        }

        /// <inheritdoc/>
        public bool TryWithLock(Action<TWrite> action)
        {
            if (TryAcquire(0))
            {
                try
                {
                    action(writer);
                }
                catch (Exception e)
                {
                    Release(e);
                    throw;
                }
                BarelyUnlock();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryWithLock(int millisecondsTimeout, Action<TWrite> action)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                try
                {
                    action(writer);
                }
                catch (Exception e)
                {
                    Release(e);
                    throw;
                }
                BarelyUnlock();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryWithLock(TimeSpan timeout, Action<TWrite> action)
        {
            if (TryAcquire(timeout))
            {
                try
                {
                    action(writer);
                }
                catch (Exception e)
                {
                    Release(e);
                    throw;
                }
                BarelyUnlock();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public GuardedValue<TWrite>? TryLock()
        {
            if (TryAcquire(0))
            {
                return new GuardedValue<TWrite>(writer, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public GuardedValue<TWrite>? TryLock(int millisecondsTimeout)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                return new GuardedValue<TWrite>(writer, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public GuardedValue<TWrite>? TryLock(TimeSpan timeout)
        {
            if (TryAcquire(timeout))
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
