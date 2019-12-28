using System;
using System.Collections;
using System.Collections.Generic;

namespace InSync
{
    /// <summary>
    /// Holds some values and cleans up when <seealso cref="IDisposable.Dispose"/> is called.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    public class GuardedMultiValue<T> : IReadOnlyList<T>, IDisposable
    {
        /// <summary>
        /// Initializes a new <seealso cref="GuardedMultiValue{T}"/> with the specified values and clean-up action. The values are casted to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="values">The value to hold.</param>
        /// <param name="dispose">The clean-up action.</param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidCastException"><paramref name="values"/> contains an object which is not <typeparamref name="T"/>.</exception>
        public GuardedMultiValue(IEnumerable<object> values, Action dispose)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            var typedValues = new List<T>();
            foreach (var o in values)
            {
                typedValues.Add((T)o);
            }
            this.values = typedValues;
            this.dispose = dispose;
        }

        private readonly IReadOnlyList<T> values;
        private readonly Action dispose;

        /// <inheritdoc/>
        public int Count => values.Count;

        /// <inheritdoc/>
        public T this[int index] => values[index];

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Performs the clean-up action.
        /// </summary>
        /// <param name="disposing"></param>
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
