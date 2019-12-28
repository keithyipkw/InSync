using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    /// <summary>
    /// Allows its protected object to be accessed only after a synchronization begins. Its methods automatically complete synchronizations or provide some means to do so. They properly handle exceptions thrown by starts of synchronization or users' operations.
    /// <para>Asynchronous operations are not supported. The protected object is non-null.</para>
    /// </summary>
    /// <typeparam name="T">The type of the protected object.</typeparam>
    public interface ISynchronized<T> where T : class
    {
        /// <summary>
        /// Locks, performs the action then unlocks. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="action">The protected object is supplied as the argument of the action</param>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        void WithLock(Action<T> action);

        /// <summary>
        /// Locks, call the function then unlocks. The returned value from the function is returned. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">The protected object is supplied as the argument of the function. The returned value is returned by this method.</param>
        /// <returns>The returned value from the function.</returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        TResult WithLock<TResult>(Func<T, TResult> func);

        /// <summary>
        /// Tries to lock and performs the action then unlocks. If the lock is not acquired, this method returns immediately. If the action throws an exception, the lock is released automatically.
        /// </summary>
        /// <param name="action">The protected object is supplied as the argument of the action</param>
        /// <returns><c>true</c> the action is performed.</returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        bool TryWithLock(Action<T> action);

        /// <summary>
        /// Tries to lock and performs the action then unlocks.  If the lock is not acquired, this method returns immediately. The returned value from the function is stored into the parameter <paramref name="result"/>. If the function throws an exception, the lock is released automatically.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">The protected object is supplied as the argument of the function. The returned value is stored into the parameter <paramref name="result"/>.</param>
        /// <param name="result">Stores the returned values from the function if the lock is acquired. It stores the default values of <typeparamref name="TResult"/> if the lock is not acquired.</param>
        /// <returns><c>true</c> the function is called.</returns>
        /// <exception cref="LockException"></exception>
        /// <exception cref="UnlockException"></exception>
        bool TryWithLock<TResult>(Func<T, TResult> func, out TResult result);

        /// <summary>
        /// Locks and returns a guard to allow access of the protected object and unlocking.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LockException"></exception>
        GuardedValue<T> Lock();

        /// <summary>
        /// Tries to lock and returns a guard to allow access of the protected object and unlocking. If the lock is not acquired, this method returns null immediately.
        /// </summary>
        /// <returns>Null if the lock is not acquired.</returns>
        /// <exception cref="LockException"></exception>
        GuardedValue<T> TryLock();
    }
}
