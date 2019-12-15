using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    public interface ISynchronized<T> where T : class
    {
        void WithLock(Action<T> action);

        TResult WithLock<TResult>(Func<T, TResult> func);

        bool TryWithLock(Action<T> action);

        bool TryWithLock<TResult>(Func<T, TResult> func, out TResult result);

        GuardedValue<T> Lock();

        GuardedValue<T> TryLock();
    }
}
