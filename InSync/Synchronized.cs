using System;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public class Synchronized<T> : ISynchronized<T>, IBareLock<T> where T : class
    {
        public Synchronized(object padLock, T value)
        {
            this.padLock = padLock ?? throw new ArgumentNullException(nameof(padLock));
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        protected readonly object padLock;
        protected readonly T value;

        public T BarelyLock()
        {
            Monitor.Enter(padLock);
            return value;
        }
        
        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        public bool BarelyTryLock(out T value)
        {
            if (Monitor.TryEnter(padLock))
            {
                value = this.value;
                return true;
            }
            value = null;
            return false;
        }

        public bool BarelyTryLock(out object value)
        {
            var result = BarelyTryLock(out T tmp);
            value = tmp;
            return result;
        }

        public void BarelyUnlock()
        {
            Monitor.Exit(padLock);
        }

        public void WithLock(Action<T> action)
        {
            Monitor.Enter(padLock);
            try
            {
                action(value);
            }
            finally
            {
                Monitor.Exit(padLock);
            }
        }
        
        public TResult WithLock<TResult>(Func<T, TResult> func)
        {
            Monitor.Enter(padLock);
            try
            {
                return func(value);
            }
            finally
            {
                Monitor.Exit(padLock);
            }
        }
        
        public bool TryWithLock(Action<T> action)
        {
            if (Monitor.TryEnter(padLock))
            {
                try
                {
                    action(value);
                    return true;
                }
                finally
                {
                    Monitor.Exit(padLock);
                }
            }
            return false;
        }

        public bool TryWithLock<TResult>(Func<T, TResult> func, out TResult result)
        {
            if (Monitor.TryEnter(padLock))
            {
                try
                {
                    result = func(value);
                    return true;
                }
                finally
                {
                    Monitor.Exit(padLock);
                }
            }
            result = default;
            return false;
        }

        public GuardedValue<T> Lock()
        {
            Monitor.Enter(padLock);
            return new GuardedValue<T>(value, BarelyUnlock);
        }
        
        public GuardedValue<T> TryLock()
        {
            if (Monitor.TryEnter(padLock))
            {
                return new GuardedValue<T>(value, BarelyUnlock);
            }
            return null;
        }
    }
}
