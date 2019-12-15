using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public interface IAsyncSynchronized<T> where T : class
    {
        Task WithLockAsync(Action<T> action);

        Task WithLockAsync(Action<T> action, CancellationToken cancellationToken);

        Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func);

        Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken);

        Task WithLockAsync(Func<T, Task> asyncAction);

        Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken);

        Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc);

        Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken);

        Task<GuardedValue<T>> LockAsync();

        Task<GuardedValue<T>> LockAsync(CancellationToken cancellationToken);
    }
}
