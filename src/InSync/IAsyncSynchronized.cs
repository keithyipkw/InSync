using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Allows its protected object to be accessed only after a synchronization begins. Its methods automatically complete synchronizations or provide some means to do so. They properly handle exceptions thrown by starts of synchronization or users' operations.
    /// <para>Various asynchronous operations are supported. The protected object is non-null.</para>
    /// </summary>
    /// <typeparam name="T">The type of the protected object.</typeparam>
    public interface IAsyncSynchronized<T> where T : class
    {
        /// <summary>
        /// Asynchronously locks, performs the action on the captured context or task scheduler (if any) then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="action">The protected object is supplied as the argument of the action</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Action<T> action);

        /// <summary>
        /// Asynchronously locks, performs the action then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="action">The protected object is supplied as the argument of the action</param>
        /// <param name="onCapturedContext"><c>true</c> to perform the action on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Action<T> action, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, performs the action on the captured context or task scheduler (if any) then unlocks. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="action">The protected object is supplied as the argument of the action</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Action<T> action, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously locks, synchronously performs the action then unlocks. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="action">The protected object is supplied as the argument of the action</param>
        /// <param name="cancellationToken"></param>
        /// <param name="onCapturedContext"><c>true</c> to perform the action on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Action<T> action, CancellationToken cancellationToken, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, calls the function on the captured context or task scheduler (if any) then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func);

        /// <summary>
        /// Asynchronously locks, calls the function then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <param name="onCapturedContext"><c>true</c> to calls the function on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, calls the function on the captured context or task scheduler (if any) then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously locks, synchronously calls the function then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="onCapturedContext"><c>true</c> to call the function on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, TResult> func, CancellationToken cancellationToken, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, performs the action on the captured context or task scheduler (if any) then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="asyncAction">The protected object is supplied as the argument of the action</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Func<T, Task> asyncAction);

        /// <summary>
        /// Asynchronously locks, performs the action then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="asyncAction">The protected object is supplied as the argument of the action</param>
        /// <param name="onCapturedContext"><c>true</c> to perform the action on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Func<T, Task> asyncAction, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, performs the action on the captured context or task scheduler (if any) then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="asyncAction">The protected object is supplied as the argument of the action</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously locks, performs the action then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="asyncAction">The protected object is supplied as the argument of the action</param>
        /// <param name="cancellationToken"></param>
        /// <param name="onCapturedContext"><c>true</c> to perform the action on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task WithLockAsync(Func<T, Task> asyncAction, CancellationToken cancellationToken, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, calls the function on the captured context or task scheduler (if any) then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="asyncFunc">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc);

        /// <summary>
        /// Asynchronously locks, calls the function then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="asyncFunc">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <param name="onCapturedContext"><c>true</c> to call the function on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks, calls the function on the captured context or task scheduler (if any) then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="asyncFunc">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously locks, calls the function then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="asyncFunc">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="onCapturedContext"><c>true</c> to call the function on the original context or task scheduler if there is a context.</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        Task<TResult> WithLockAsync<TResult>(Func<T, Task<TResult>> asyncFunc, CancellationToken cancellationToken, bool onCapturedContext);

        /// <summary>
        /// Asynchronously locks and returns a guard to allow access of the protected object and unlocking.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        Task<GuardedValue<T>> LockAsync();

        /// <summary>
        /// Asynchronously locks and returns a guard to allow access of the protected object and unlocking.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="LockException"></exception>
        Task<GuardedValue<T>> LockAsync(CancellationToken cancellationToken);
    }
}
