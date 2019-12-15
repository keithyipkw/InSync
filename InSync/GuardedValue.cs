using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    public class GuardedValue<T> : IDisposable
    {
        public GuardedValue(T value, Action dispose)
        {
            this.value = value;
            this.dispose = dispose;
        }

        private readonly T value;
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
