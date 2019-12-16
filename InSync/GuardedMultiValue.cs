using System;
using System.Collections;
using System.Collections.Generic;

namespace InSync
{
    public class GuardedMultiValue<T> : IReadOnlyList<T>, IDisposable
    {
        public GuardedMultiValue(IEnumerable<T> values, Action dispose)
        {
            this.values = new List<T>(values);
            this.dispose = dispose;
        }

        private readonly IReadOnlyList<T> values;
        private readonly Action dispose;

        public int Count => values.Count;

        public T this[int index] => values[index];

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

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
