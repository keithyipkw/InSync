using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    /// <summary>
    /// https://howardhinnant.github.io/dining_philosophers.html#Polite
    /// </summary>
    /// <param name="locks"></param>
    /// <param name="action"></param>
    public static class MultiSync
    {
        public static GuardedMultiValue<T> All<T>(IReadOnlyList<IBareLock<T>> locks)
            where T : class
        {
            Action dispose = null;
            try
            {
                IReadOnlyList<object> values;
                (values, dispose) = AcquireAll(locks);
                var count = values.Count;
                var typedValues = new T[count];
                for (int i = 0; i < count; ++i)
                {
                    typedValues[i] = (T)values[i];
                }
                return new GuardedMultiValue<T>(typedValues, dispose);
            }
            catch
            {
                dispose?.Invoke();
                throw;
            }
        }
        
        public static GuardedMultiValue<object> All(IReadOnlyList<IBareLock> locks)
        {
            Action dispose = null;
            try
            {
                IReadOnlyList<object> values;
                (values, dispose) = AcquireAll(locks);
                return new GuardedMultiValue<object>(values, dispose);
            }
            catch
            {
                dispose?.Invoke();
                throw;
            }
        }

        private static (IReadOnlyList<object> Values, Action Dispose) AcquireAll(IReadOnlyList<IBareLock> locks)
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return (new object[0], null);
            }

            void UnlockAll()
            {
                for (var i = 0; i < count; ++i)
                {
                    locks[i].BarelyUnlock();
                }
            }

            var maxI = count - 1;
            var values = new object[count];
            var lockedCount = 1;
            try
            {
                values[0] = locks[0].BarelyLock();
                if (count == 1)
                {
                    return (values, UnlockAll);
                }
                for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                {
                    if (locks[i].BarelyTryLock(out values[i]))
                    {
                        ++lockedCount;
                        if (lockedCount == count)
                        {
                            return (values, UnlockAll);
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
                        values[i] = locks[i].BarelyLock();
                        lockedCount = 1;
                    }
                }
            }
            catch
            {
                for (var u = 0; u < count; ++u)
                {
                    if (values[u] != null)
                    {
                        locks[u].BarelyUnlock();
                    }
                }
                throw;
            }
        }

        public static Task<GuardedMultiValue<T>> AllAsync<T>(IReadOnlyList<IBareAsyncLock<T>> locks)
            where T : class
        {
            return AllAsync(locks, CancellationToken.None);
        }

        public static async Task<GuardedMultiValue<T>> AllAsync<T>(IReadOnlyList<IBareAsyncLock<T>> locks, CancellationToken cancellationToken)
            where T : class
        {
            Action dispose = null;
            try
            {
                IReadOnlyList<object> values;
                (values, dispose) = await AcquireAllAsync(locks, cancellationToken);
                var count = values.Count;
                var typedValues = new T[count];
                for (int i = 0; i < count; ++i)
                {
                    typedValues[i] = (T)values[i];
                }
                return new GuardedMultiValue<T>(typedValues, dispose);
            }
            catch
            {
                dispose?.Invoke();
                throw;
            }
        }

        public static Task<GuardedMultiValue<object>> AllAsync(IReadOnlyList<IBareAsyncLock> locks)
        {
            return AllAsync(locks, CancellationToken.None);
        }

        public static async Task<GuardedMultiValue<object>> AllAsync(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
        {
            Action dispose = null;
            try
            {
                IReadOnlyList<object> values;
                (values, dispose) = await AcquireAllAsync(locks, cancellationToken);
                return new GuardedMultiValue<object>(values, dispose);
            }
            catch
            {
                dispose?.Invoke();
                throw;
            }
        }

        private static async Task<(IReadOnlyList<object> Values, Action Dispose)> AcquireAllAsync(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return (new object[0], null);
            }

            void UnlockAll()
            {
                for (var i = 0; i < count; ++i)
                {
                    locks[i].BarelyUnlock();
                }
            }

            var maxI = count - 1;
            var values = new object[count];
            var lockedCount = 1;
            try
            {
                values[0] = await locks[0].BarelyLockAsync(cancellationToken);
                if (count == 1)
                {
                    return (values, UnlockAll);
                }
                for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                {
                    if (locks[i].BarelyTryLock(out values[i]))
                    {
                        ++lockedCount;
                        if (lockedCount == count)
                        {
                            return (values, UnlockAll);
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
                        values[i] = await locks[i].BarelyLockAsync(cancellationToken);
                        lockedCount = 1;
                    }
                }
            }
            catch
            {
                for (var u = 0; u < count; ++u)
                {
                    if (values[u] != null)
                    {
                        locks[u].BarelyUnlock();
                    }
                }
                throw;
            }
        }
    }
}
