﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public class ReaderSynchronized<TRead> : ISynchronized<TRead>, IBareLock<TRead>
        where TRead : class
    {
        public ReaderSynchronized(ReaderWriterLockSlim readerWriterLockSlim, TRead readValue)
        {
            this.readerWriterLockSlim = readerWriterLockSlim;
            this.readValue = readValue;
        }

        protected readonly ReaderWriterLockSlim readerWriterLockSlim;
        protected readonly TRead readValue;
        
        public TRead BarelyLock()
        {
            readerWriterLockSlim.EnterReadLock();
            return readValue;
        }

        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }
        
        public bool BarelyTryLock(out TRead value)
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                value = this.readValue;
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
            readerWriterLockSlim.ExitReadLock();
        }
        
        public void WithLock(Action<TRead> action)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                action(readValue);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }
        
        public TResult WithLock<TResult>(Func<TRead, TResult> func)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                return func(readValue);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }
        
        public GuardedValue<TRead> Lock()
        {
            readerWriterLockSlim.EnterReadLock();
            return new GuardedValue<TRead>(readValue, BarelyUnlock);
        }
        
        public bool TryWithLock(Action<TRead> action)
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                try
                {
                    action(readValue);
                    return true;
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
            return false;
        }
        
        public bool TryWithLock<TResult>(Func<TRead, TResult> func, out TResult result)
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                try
                {
                    result = func(readValue);
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
        
        public GuardedValue<TRead> TryLock()
        {
            if (readerWriterLockSlim.TryEnterReadLock(0))
            {
                return new GuardedValue<TRead>(readValue, BarelyUnlock);
            }
            return null;
        }
    }
}
