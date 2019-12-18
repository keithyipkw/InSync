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
            return AcquireAll<T>(locks);
        }
        
        public static GuardedMultiValue<object> All(IReadOnlyList<IBareLock> locks)
        {
            return AcquireAll<object>(locks);
        }

        private static GuardedMultiValue<T> AcquireAll<T>(IReadOnlyList<IBareLock> locks)
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return new GuardedMultiValue<T>(new object[0], null);
            }

            void UnlockAll()
            {
                List<Exception> exceptions = null;
                for (var i = 0; i < count; ++i)
                {
                    try
                    {
                        locks[i].BarelyUnlock();
                    }
                    catch (Exception e)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }
                        exceptions.Add(e);
                    }
                }
                if (exceptions != null)
                {
                    throw new UnlockException(exceptions, "Exceptions occurred when releasing locks");
                }
            }

            var maxI = count - 1;
            var values = new object[count];
            var lockedCount = 1;
            try
            {
                values[0] = locks[0].BarelyLock();
                if (count > 1)
                {
                    for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                    {
                        if (locks[i].BarelyTryLock(out values[i]))
                        {
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
                            values[i] = locks[i].BarelyLock();
                            lockedCount = 1;
                        }
                    }
                }
                return new GuardedMultiValue<T>(values, UnlockAll);
            }
            catch (Exception e)
            {
                List<Exception> exceptions = null;
                for (var u = 0; u < count; ++u)
                {
                    if (values[u] != null)
                    {
                        try
                        {
                            locks[u].BarelyUnlock();
                        }
                        catch (Exception unlockException)
                        {
                            if (exceptions == null)
                            {
                                exceptions = new List<Exception>();
                            }
                            exceptions.Add(unlockException);
                        }
                    }
                }
                if (exceptions != null)
                {
                    throw new UnlockException(e, exceptions, "An exception occurred then other exceptions occurred when releasing locks");
                }
                throw;
            }
        }

        public static Task<GuardedMultiValue<T>> AllAsync<T>(IReadOnlyList<IBareAsyncLock<T>> locks)
            where T : class
        {
            return AllAsync(locks, CancellationToken.None);
        }

        public static Task<GuardedMultiValue<T>> AllAsync<T>(IReadOnlyList<IBareAsyncLock<T>> locks, CancellationToken cancellationToken)
            where T : class
        {
            return AcquireAllAsync<T>(locks, cancellationToken);
        }

        public static Task<GuardedMultiValue<object>> AllAsync(IReadOnlyList<IBareAsyncLock> locks)
        {
            return AllAsync(locks, CancellationToken.None);
        }

        public static Task<GuardedMultiValue<object>> AllAsync(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
        {
            return AcquireAllAsync<object>(locks, cancellationToken);
        }

        private static async Task<GuardedMultiValue<T>> AcquireAllAsync<T>(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return new GuardedMultiValue<T>(new object[0], null);
            }

            void UnlockAll()
            {
                List<Exception> exceptions = null;
                for (var i = 0; i < count; ++i)
                {
                    try
                    {
                        locks[i].BarelyUnlock();
                    }
                    catch (Exception e)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }
                        exceptions.Add(e);
                    }
                }
                if (exceptions != null)
                {
                    throw new UnlockException(exceptions, "Exceptions occurred when releasing locks");
                }
            }

            var maxI = count - 1;
            var values = new object[count];
            var lockedCount = 1;
            try
            {
                values[0] = await locks[0].BarelyLockAsync(cancellationToken);
                if (count > 1)
                {
                    for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                    {
                        if (locks[i].BarelyTryLock(out values[i]))
                        {
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
                            values[i] = await locks[i].BarelyLockAsync(cancellationToken);
                            lockedCount = 1;
                        }
                    }
                }
                return new GuardedMultiValue<T>(values, UnlockAll);
            }
            catch (Exception e)
            {
                List<Exception> exceptions = null;
                for (var u = 0; u < count; ++u)
                {
                    if (values[u] != null)
                    {
                        try
                        {
                            locks[u].BarelyUnlock();
                        }
                        catch (Exception unlockException)
                        {
                            if (exceptions == null)
                            {
                                exceptions = new List<Exception>();
                            }
                            exceptions.Add(unlockException);
                        }
                    }
                }
                if (exceptions != null)
                {
                    throw new UnlockException(e, exceptions, "An exception occurred then other exceptions occurred when releasing locks");
                }
                throw;
            }
        }
    }
}
