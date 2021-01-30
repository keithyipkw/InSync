using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        
        private void Acquire()
        {
            try
            {
                readerWriterLockSlim.EnterReadLock();
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
                return readerWriterLockSlim.TryEnterReadLock(millisecondsTimeout);
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
                return readerWriterLockSlim.TryEnterReadLock(timeout);
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
                readerWriterLockSlim.ExitReadLock();
            }
            catch (Exception e)
            {
                throw new UnlockException(priorException, e);
            }
        }

        /// <inheritdoc/>
        public TRead BarelyLock()
        {
            Acquire();
            return reader;
        }

        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        /// <inheritdoc/>
        public bool BarelyTryLock([NotNullWhen(true)] out TRead? value)
        {
            if (TryAcquire(0))
            {
                value = this.reader;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(int millisecondsTimeout, [NotNullWhen(true)] out TRead? value)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                value = this.reader;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        public bool BarelyTryLock(TimeSpan timeout, [NotNullWhen(true)] out TRead? value)
        {
            if (TryAcquire(timeout))
            {
                value = this.reader;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc/>
        bool IBareLock.BarelyTryLock([NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(out TRead? tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        bool IBareLock.BarelyTryLock(int millisecondsTimeout, [NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(millisecondsTimeout, out TRead? tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        bool IBareLock.BarelyTryLock(TimeSpan timeout, [NotNullWhen(true)] out object? value)
        {
            var result = BarelyTryLock(timeout, out TRead? tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        public void BarelyUnlock()
        {
            try
            {
                readerWriterLockSlim.ExitReadLock();
            }
            catch (Exception e)
            {
                throw new UnlockException(e);
            }
        }

        /// <inheritdoc/>
        public void WithLock(Action<TRead> action)
        {
            Acquire();
            try
            {
                action(reader);
            }
            catch (Exception e)
            {
                Release(e);
                throw;
            }
            BarelyUnlock();
        }

        /// <inheritdoc/>
        public TResult WithLock<TResult>(Func<TRead, TResult> func)
        {
            Acquire();
            TResult result;
            try
            {
                result = func(reader);
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
        public GuardedValue<TRead> Lock()
        {
            Acquire();
            return new GuardedValue<TRead>(reader, BarelyUnlock);
        }

        /// <inheritdoc/>
        public bool TryWithLock(Action<TRead> action)
        {
            if (TryAcquire(0))
            {
                try
                {
                    action(reader);
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
        public bool TryWithLock(int millisecondsTimeout, Action<TRead> action)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                try
                {
                    action(reader);
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
        public bool TryWithLock(TimeSpan timeout, Action<TRead> action)
        {
            if (TryAcquire(timeout))
            {
                try
                {
                    action(reader);
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
        public GuardedValue<TRead>? TryLock()
        {
            if (TryAcquire(0))
            {
                return new GuardedValue<TRead>(reader, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public GuardedValue<TRead>? TryLock(int millisecondsTimeout)
        {
            if (TryAcquire(millisecondsTimeout))
            {
                return new GuardedValue<TRead>(reader, BarelyUnlock);
            }
            return null;
        }

        /// <inheritdoc/>
        public GuardedValue<TRead>? TryLock(TimeSpan timeout)
        {
            if (TryAcquire(timeout))
            {
                return new GuardedValue<TRead>(reader, BarelyUnlock);
            }
            return null;
        }
    }
}
