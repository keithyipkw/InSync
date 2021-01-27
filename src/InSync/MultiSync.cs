using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Acquires the locks unorderly. See <seealso cref="MultiSync"/> for more details.
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
        /// Acquires the locks unorderly. <seealso cref="TimingMethod.EnvironmentTick"/> is good enough for general usages. This method detects an increase in remaining time and continue the counting from there.<br/>
        /// Although <seealso cref="TimingMethod.EnvironmentTick"/> uses <seealso cref="Environment.TickCount"/> which is 32-bits (representing 49.8 days), it is practically safe. The wrap around problem is only effective if an execution is suspended for 49.8 days subtracting the timeout, at least 24.9 days.<br/>
        /// <seealso cref="TimingMethod.DateTime"/> is affected by system clock changes. A clock may change several minutes for a low quality system in a network time synchronization. An user may change their system clock to an arbitrary time.<br/>
        /// See <seealso cref="MultiSync"/> for more details.<br/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locks">The locks to acquire.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait. A negative value specifies an infinite wait.</param>
        /// <param name="timingMethod">The method of counting time. See <seealso cref="TimingMethod"/> for more details.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects if all the locks are acquired; otherwise, <code>null</code> is returned.</returns>
        public static GuardedValue<IReadOnlyList<T>> All<T>(IReadOnlyList<IBareLock<T>> locks, int millisecondsTimeout, TimingMethod timingMethod)
            where T : class
        {
            return AcquireAll<T>(locks, millisecondsTimeout, timingMethod);
        }

        /// <summary>
        /// Acquires the locks unorderly. See <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <param name="locks">The locks to acquire.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static GuardedValue<IReadOnlyList<object>> All(IReadOnlyList<IBareLock> locks)
        {
            return AcquireAll<object>(locks);
        }

        /// <summary>
        /// Acquires the locks unorderly. <seealso cref="TimingMethod.EnvironmentTick"/> is good enough for general usages. This method detects an increase in remaining time and continue the counting from there.<br/>
        /// Although <seealso cref="TimingMethod.EnvironmentTick"/> uses <seealso cref="Environment.TickCount"/> which is 32-bits (representing 49.8 days), it is practically safe. The wrap around problem is only effective if an execution is suspended for 49.8 days subtracting the timeout, at least 24.9 days.<br/>
        /// <seealso cref="TimingMethod.DateTime"/> is affected by system clock changes. A clock may change several minutes for a low quality system in a network time synchronization. An user may change their system clock to an arbitrary time.<br/>
        /// See <seealso cref="MultiSync"/> for more details.<br/>
        /// </summary>
        /// <param name="locks">The locks to acquire.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait. A negative value specifies an infinite wait.</param>
        /// <param name="timingMethod">The method of counting time. See <seealso cref="TimingMethod"/> for more details.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects if all the locks are acquired; otherwise, <code>null</code> is returned.</returns>
        public static GuardedValue<IReadOnlyList<object>> All(IReadOnlyList<IBareLock> locks, int millisecondsTimeout, TimingMethod timingMethod)
        {
            return AcquireAll<object>(locks, millisecondsTimeout, timingMethod);
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
                            Unlock(null);
                            for (var u = 0; u < count; ++u)
                            {
                                values[u] = null;
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

        private static GuardedValue<IReadOnlyList<T>> AcquireAll<T>(IReadOnlyList<IBareLock> locks, int millisecondsTimeout, TimingMethod timingMethod)
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

            var beginTicks = 0;
            Stopwatch stopwatch = null;
            DateTime beginDateTime = DateTime.MinValue;
            if (millisecondsTimeout > 0)
            {
                switch (timingMethod)
                {
                    case TimingMethod.Stopwatch:
                        stopwatch = new Stopwatch();
                        stopwatch.Start();
                        break;
                    case TimingMethod.EnvironmentTick:
                        beginTicks = Environment.TickCount;
                        break;
                    case TimingMethod.DateTime:
                        beginDateTime = DateTime.UtcNow;
                        break;
                }
            }

            if (locks[0].BarelyTryLock(millisecondsTimeout, out var value))
            {
                values[0] = (T)value;
            }
            else
            {
                return null;
            }

            try
            {
                if (count > 1)
                {
                    var maxI = count - 1;
                    var lockedCount = 1;
                    var previousRemainingTimeMs = millisecondsTimeout;
                    for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                    {
                        if (locks[i].BarelyTryLock(out value))
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
                            Unlock(null);
                            for (var u = 0; u < count; ++u)
                            {
                                values[u] = null;
                            }
                            Thread.Yield();

                            int remainingTimeMs;
                            if (millisecondsTimeout > 0)
                            {
                                if (timingMethod == TimingMethod.Stopwatch)
                                {
                                    var elapsed = stopwatch.ElapsedMilliseconds;
                                    if (elapsed >= int.MaxValue)
                                    {
                                        remainingTimeMs = 0;
                                    }
                                    else
                                    {
                                        remainingTimeMs = millisecondsTimeout - (int)elapsed;
                                        remainingTimeMs = remainingTimeMs > 0 ? remainingTimeMs : 0;
                                    }
                                }
                                else if (timingMethod == TimingMethod.EnvironmentTick)
                                {
                                    unchecked
                                    {
                                        // millisecondsTimeout is non-negative here
                                        remainingTimeMs = beginTicks + millisecondsTimeout - Environment.TickCount;
                                        if (remainingTimeMs > previousRemainingTimeMs)
                                        {
                                            beginTicks -= remainingTimeMs - previousRemainingTimeMs;
                                            remainingTimeMs = previousRemainingTimeMs;
                                        }
                                        remainingTimeMs = remainingTimeMs > 0 ? remainingTimeMs : 0;
                                    }
                                }
                                else if (timingMethod == TimingMethod.DateTime)
                                {
                                    var elapsed = (DateTime.UtcNow - beginDateTime).TotalMilliseconds;
                                    if (elapsed >= int.MaxValue)
                                    {
                                        remainingTimeMs = 0;
                                    }
                                    else
                                    {
                                        remainingTimeMs = millisecondsTimeout - (int)elapsed;
                                        if (remainingTimeMs > previousRemainingTimeMs)
                                        {
                                            beginDateTime -= TimeSpan.FromMilliseconds(remainingTimeMs - previousRemainingTimeMs);
                                            remainingTimeMs = previousRemainingTimeMs;
                                        }
                                        remainingTimeMs = remainingTimeMs > 0 ? remainingTimeMs : 0;
                                    }
                                }
                                else
                                {
                                    remainingTimeMs = -1;
                                }
                            }
                            else
                            {
                                remainingTimeMs = millisecondsTimeout;
                            }
                            previousRemainingTimeMs = remainingTimeMs;

                            if (locks[i].BarelyTryLock(remainingTimeMs, out value))
                            {
                                values[i] = (T)value;
                                lockedCount = 1;
                            }
                            else
                            {
                                Unlock(null);
                                return null;
                            }
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
        /// Acquires the locks unorderly. See <seealso cref="MultiSync"/> for more details.
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
        /// Acquires the locks unorderly. See <seealso cref="MultiSync"/> for more details.
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
        /// Acquires the locks unorderly. See <seealso cref="MultiSync"/> for more details.
        /// </summary>
        /// <param name="locks">The locks to acquire.</param>
        /// <returns>An <seealso cref="IDisposable"/> containing the protected objects.</returns>
        public static Task<GuardedValue<IReadOnlyList<object>>> AllAsync(IReadOnlyList<IBareAsyncLock> locks)
        {
            return AllAsync(locks, CancellationToken.None);
        }

        /// <summary>
        /// Acquires the locks unorderly. See <seealso cref="MultiSync"/> for more details.
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
                            Unlock(null);
                            for (var u = 0; u < count; ++u)
                            {
                                values[u] = null;
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
