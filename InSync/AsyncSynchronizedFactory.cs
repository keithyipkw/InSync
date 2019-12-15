using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSync
{
    public static class AsyncSynchronized
    {
        public static AsyncSynchronized<T> Create<T>(T value) where T : class
        {
            return new AsyncSynchronized<T>(new SemaphoreSlim(1), value);
        }

        public static AsyncSynchronized<T> Create<T>(SemaphoreSlim semaphore, T value) where T : class
        {
            return new AsyncSynchronized<T>(semaphore, value);
        }
    }
}
