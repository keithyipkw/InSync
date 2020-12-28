using System;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Uses <seealso cref="Monitor"/> to synchronize and only expose its protected object after a synchronization begins.
    /// <para>Asynchronous operations are not supported. The protected object is non-null.</para>
    /// </summary>
    /// <typeparam name="T">The type of the protected object.</typeparam>
    public class Synchronized<T> : ISynchronized<T>, IBareLock<T> where T : class
    {
        /// <summary>
        /// Initializes a new <seealso cref="Synchronized{T}"/> with the specified lock and the object to protect.
        /// </summary>
        /// <param name="padLock">The lock for synchronization</param>
        /// <param name="value">The object to protect.</param>
        /// <exception cref="ArgumentNullException"><paramref name="padLock"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public Synchronized(object padLock, T value)
        {
            this.padLock = padLock ?? throw new ArgumentNullException(nameof(padLock));
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The lock for synchronization
        /// </summary>
        protected readonly object padLock;

        /// <summary>
        /// The non-null object to protect.
        /// </summary>
        protected readonly T value;
        
        /// <inheritdoc/>
        public T BarelyLock()
        {
            Monitor.Enter(padLock);
            return value;
        }
        
        object IBareLock.BarelyLock()
        {
            return BarelyLock();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool BarelyTryLock(out object value)
        {
            var result = BarelyTryLock(out T tmp);
            value = tmp;
            return result;
        }

        /// <inheritdoc/>
        public void BarelyUnlock()
        {
            try
            {
                Monitor.Exit(padLock);
            }
            catch (Exception e)
            {
                throw new UnlockException(e);
            }
        }
        
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        public GuardedValue<T> Lock()
        {
            Monitor.Enter(padLock);
            return new GuardedValue<T>(value, BarelyUnlock);
        }

        /// <inheritdoc/>
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
