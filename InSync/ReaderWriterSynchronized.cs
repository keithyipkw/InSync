using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public class ReaderWriterSynchronized<TWrite, TRead> : ISynchronized<TWrite>, IBareLock<TWrite>
        where TWrite : class
        where TRead : class
    {
        public ReaderWriterSynchronized(ReaderWriterLockSlim readerWriterLockSlim, TWrite writeValue, TRead readValue)
        {
            this.readerWriterLockSlim = readerWriterLockSlim;
            this.writeValue = writeValue;
            this.readValue = readValue;
            Reader = new ReaderSynchronized<TRead>(readerWriterLockSlim, readValue);
            UpgradeableReader = new UpgradeableReaderSynchronized<TRead>(readerWriterLockSlim, readValue);
        }

        protected readonly ReaderWriterLockSlim readerWriterLockSlim;
        protected readonly TWrite writeValue;
        protected readonly TRead readValue;

        public ReaderSynchronized<TRead> Reader { get; }
        public UpgradeableReaderSynchronized<TRead> UpgradeableReader { get; }

        public TWrite BarelyLock()
        {
            readerWriterLockSlim.EnterWriteLock();
            return writeValue;
        }
        
        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }
        
        public bool BarelyTryLock(out TWrite value)
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                value = this.writeValue;
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

        public void BarelyUnlock()
        {
            readerWriterLockSlim.ExitWriteLock();
        }
        
        public void WithLock(Action<TWrite> action)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                action(writeValue);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }
        
        public TResult WithLock<TResult>(Func<TWrite, TResult> func)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                return func(writeValue);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }
        
        public GuardedValue<TWrite> Lock()
        {
            readerWriterLockSlim.EnterWriteLock();
            return new GuardedValue<TWrite>(writeValue, BarelyUnlock);
        }
        
        public bool TryWithLock(Action<TWrite> action)
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                try
                {
                    action(writeValue);
                    return true;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
            return false;
        }
        
        public bool TryWithLock<TResult>(Func<TWrite, TResult> func, out TResult result)
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                try
                {
                    result = func(writeValue);
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
        
        public GuardedValue<TWrite> TryLock()
        {
            if (readerWriterLockSlim.TryEnterWriteLock(0))
            {
                return new GuardedValue<TWrite>(writeValue, BarelyUnlock);
            }
            return null;
        }
    }

    public class ReaderWriterSynchronized<TWrite> : ReaderWriterSynchronized<TWrite, TWrite>
        where TWrite : class
    {
        public ReaderWriterSynchronized(ReaderWriterLockSlim readerWriterLockSlim, TWrite writeValue) : base(readerWriterLockSlim, writeValue, writeValue)
        {
        }
    }
}
