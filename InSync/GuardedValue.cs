using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    /// <summary>
    /// Holds a value and cleans up when <seealso cref="IDisposable.Dispose"/> is called.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class GuardedValue<T> : IDisposable
    {
        /// <summary>
        /// Initializes a new <seealso cref="GuardedValue{T}"/> with the specified value and clean-up action.
        /// </summary>
        /// <param name="value">The value to hold.</param>
        /// <param name="dispose">The clean-up action.</param>
        public GuardedValue(T value, Action dispose)
        {
            this.value = value;
            this.dispose = dispose;
        }

        private readonly T value;

        /// <summary>
        /// Gets the value it is holding.
        /// </summary>
        public T Value
        {
            get
            {
                if (disposedValue)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return value;
            }
        }

        private readonly Action dispose;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <ineritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dispose?.Invoke();
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs the clean-up action.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
