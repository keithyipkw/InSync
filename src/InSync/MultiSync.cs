﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// Provides easy ways to acquire multiple locks without deadlocks. It does not require any setup nor lock organizations. Fairness is thread-based and provided by the OS because <see cref="Thread.Yield"/> is used. Livelock may occur for a very short period under high contention. In such case, CPU power is wasted.
    /// <para>It uses the smart and polite method described in https://howardhinnant.github.io/dining_philosophers.html#Polite. Basically, it tries to acquire the locks one by one. If an acquisition fails, it releases all acquired locks. Before a blocking retry of the last acquisition, it yields to let other threads to process first.</para>
    /// </summary>
    public static class MultiSync
    {
        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <typeparam name="T">The type of the protected objects.</typeparam>
        /// <param name="locks">The locks to acquire.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static GuardedValue<IReadOnlyList<T>> All<T>(IReadOnlyList<IBareLock<T>> locks)
            where T : class
        {
            return AcquireAll<T>(locks);
        }

        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <param name="locks">The locks to acquire.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static GuardedValue<IReadOnlyList<object>> All(IReadOnlyList<IBareLock> locks)
        {
            return AcquireAll<object>(locks);
        }
        
        private static GuardedValue<IReadOnlyList<T>> AcquireAll<T>(IReadOnlyList<IBareLock> locks)
            where T : class
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return new GuardedValue<IReadOnlyList<T>>(new T[0], null);
            }

            var values = new T[count];
            void Unlock(Exception priorException)
            {
                Dictionary<int, Exception> exceptions = null;
                for (var i = 0; i < count; ++i)
                {
                    if (values[i] != null)
                    {
                        try
                        {
                            locks[i].BarelyUnlock();
                        }
                        catch (UnlockException e)
                        {
                            if (exceptions == null)
                            {
                                exceptions = new Dictionary<int, Exception>();
                            }
                            exceptions[i] = e.InnerException;
                        }
                        catch (Exception e)
                        {
                            if (exceptions == null)
                            {
                                exceptions = new Dictionary<int, Exception>();
                            }
                            exceptions[i] = e;
                        }
                    }
                }
                if (exceptions != null)
                {
                    throw new UnlockException(priorException, exceptions);
                }
            }

            var maxI = count - 1;
            var lockedCount = 1;
            values[0] = (T)locks[0].BarelyLock();
            try
            {
                if (count > 1)
                {
                    for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                    {
                        if (locks[i].BarelyTryLock(out var value))
                        {
                            values[i] = (T)value;
                            ++lockedCount;
                            if (lockedCount == count)
                            {
                                break;
                            }
                        }
                        else
                        {
                            for (var u = 0; u < count; ++u)
                            {
                                if (values[u] != null)
                                {
                                    values[u] = null;
                                    locks[u].BarelyUnlock();
                                }
                            }
                            Thread.Yield();
                            values[i] = (T)locks[i].BarelyLock();
                            lockedCount = 1;
                        }
                    }
                }
                return new GuardedValue<IReadOnlyList<T>>(values, () => Unlock(null));
            }
            catch (Exception e)
            {
                Unlock(e);
                throw;
            }
        }

        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <typeparam name="T">The type of the protected objects.</typeparam>
        /// <param name="locks">The locks to acquire.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static Task<GuardedValue<IReadOnlyList<T>>> AllAsync<T>(IReadOnlyList<IBareAsyncLock<T>> locks)
            where T : class
        {
            return AllAsync(locks, CancellationToken.None);
        }

        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <typeparam name="T">The type of the protected objects.</typeparam>
        /// <param name="locks">The locks to acquire.</param>
        /// <param name="cancellationToken">A <seealso cref="CancellationToken"/> to observe while waiting for the tasks to complete.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static Task<GuardedValue<IReadOnlyList<T>>> AllAsync<T>(IReadOnlyList<IBareAsyncLock<T>> locks, CancellationToken cancellationToken)
            where T : class
        {
            return AcquireAllAsync<T>(locks, cancellationToken);
        }

        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <param name="locks">The locks to acquire.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static Task<GuardedValue<IReadOnlyList<object>>> AllAsync(IReadOnlyList<IBareAsyncLock> locks)
        {
            return AllAsync(locks, CancellationToken.None);
        }

        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <param name="locks">The locks to acquire.</param>
        /// <param name="cancellationToken">A <seealso cref="CancellationToken"/> to observe while waiting for the tasks to complete.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static Task<GuardedValue<IReadOnlyList<object>>> AllAsync(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
        {
            return AcquireAllAsync<object>(locks, cancellationToken);
        }
        
        private static async Task<GuardedValue<IReadOnlyList<T>>> AcquireAllAsync<T>(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
            where T : class
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return new GuardedValue<IReadOnlyList<T>>(new T[0], null);
            }

            var values = new T[count];
            void Unlock(Exception priorException)
            {
                Dictionary<int, Exception> exceptions = null;
                for (var i = 0; i < count; ++i)
                {
                    if (values[i] != null)
                    {
                        try
                        {
                            locks[i].BarelyUnlock();
                        }
                        catch (UnlockException e)
                        {
                            if (exceptions == null)
                            {
                                exceptions = new Dictionary<int, Exception>();
                            }
                            exceptions[i] = e.InnerException;
                        }
                        catch (Exception e)
                        {
                            if (exceptions == null)
                            {
                                exceptions = new Dictionary<int, Exception>();
                            }
                            exceptions[i] = e;
                        }
                    }
                }
                if (exceptions != null)
                {
                    throw new UnlockException(priorException, exceptions);
                }
            }

            var maxI = count - 1;
            var lockedCount = 1;
            values[0] = (T)await locks[0].BarelyLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (count > 1)
                {
                    for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                    {
                        if (locks[i].BarelyTryLock(out var value))
                        {
                            values[i] = (T)value;
                            ++lockedCount;
                            if (lockedCount == count)
                            {
                                break;
                            }
                        }
                        else
                        {
                            for (var u = 0; u < count; ++u)
                            {
                                if (values[u] != null)
                                {
                                    values[u] = null;
                                    locks[u].BarelyUnlock();
                                }
                            }
                            Task<object> Acquire()
                            {
                                return locks[i].BarelyLockAsync(cancellationToken);
                            }
                            values[i] = (T)await (await Task.Factory.StartNew(Acquire,
                                CancellationToken.None,
                                TaskCreationOptions.PreferFairness,
                                TaskScheduler.Default).ConfigureAwait(false))
                                .ConfigureAwait(false);
                            lockedCount = 1;
                        }
                    }
                }
                return new GuardedValue<IReadOnlyList<T>>(values, () => Unlock(null));
            }
            catch (Exception e)
            {
                Unlock(e);
                throw;
            }
        }
    }
}
