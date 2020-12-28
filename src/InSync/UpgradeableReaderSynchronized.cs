using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Represents the upgradeable reader part of <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/>.
    /// </summary>
    /// <typeparam name="TRead">The type of the reader to protect.</typeparam>
    public class UpgradeableReaderSynchronized<TRead> : ISynchronized<TRead>, IBareLock<TRead>
        where TRead : class
    {
        /// <summary>
        /// Initialize a <seealso cref="UpgradeableReaderSynchronized{TRead}"/> with the specified <seealso cref="readerWriterLockSlim"/> and reader.
        /// </summary>
        /// <param name="readerWriterLockSlim"></param>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException"><paramref name="readerWriterLockSlim"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public UpgradeableReaderSynchronized(ReaderWriterLockSlim readerWriterLockSlim, TRead reader)
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
                readerWriterLockSlim.EnterUpgradeableReadLock();
            }
            catch (Exception e)
            {
                throw new LockException(e);
            }
        }

        private bool TryAcquire()
        {
            try
            {
                return readerWriterLockSlim.TryEnterUpgradeableReadLock(0);
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
                readerWriterLockSlim.ExitUpgradeableReadLock();
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
        public bool BarelyTryLock(out TRead value)
        {
            if (TryAcquire())
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
            try
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
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
            if (TryAcquire())
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
        public GuardedValue<TRead> TryLock()
        {
            if (TryAcquire())
            {
                return new GuardedValue<TRead>(reader, BarelyUnlock);
            }
            return null;
        }
    }
}
