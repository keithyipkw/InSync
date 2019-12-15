using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync
{
    public static class MultiSync
    {
        /// <summary>
        /// https://howardhinnant.github.io/dining_philosophers.html#Polite
        /// </summary>
        /// <param name="locks"></param>
        /// <param name="action"></param>
        public static GuardedValue<IReadOnlyList<object>> All(IReadOnlyList<IBareLock> locks)
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return new GuardedValue<IReadOnlyList<object>>(new object[0], null);
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
                    return new GuardedValue<IReadOnlyList<object>>(values, UnlockAll);
                }
                for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                {
                    if (locks[i].BarelyTryLock(out values[i]))
                    {
                        ++lockedCount;
                        if (lockedCount == count)
                        {
                            return new GuardedValue<IReadOnlyList<object>>(values, UnlockAll);
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
            finally
            {
                if (lockedCount != count)
                {
                    for (var u = 0; u < count; ++u)
                    {
                        if (values[u] != null)
                        {
                            locks[u].BarelyUnlock();
                        }
                    }
                }
            }
        }
        
        public static Task<GuardedValue<IReadOnlyList<object>>> AllAsync(IReadOnlyList<IBareAsyncLock> locks)
        {
            return AllAsync(locks, CancellationToken.None);
        }

        public static async Task<GuardedValue<IReadOnlyList<object>>> AllAsync(IReadOnlyList<IBareAsyncLock> locks, CancellationToken cancellationToken)
        {
            if (locks == null)
            {
                throw new ArgumentNullException(nameof(locks));
            }
            var count = locks.Count;
            if (count == 0)
            {
                return new GuardedValue<IReadOnlyList<object>>(new object[0], null);
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
                    return new GuardedValue<IReadOnlyList<object>>(values, UnlockAll);
                }
                for (var i = 1; ; i = i >= maxI ? 0 : i + 1)
                {
                    if (locks[i].BarelyTryLock(out values[i]))
                    {
                        ++lockedCount;
                        if (lockedCount == count)
                        {
                            return new GuardedValue<IReadOnlyList<object>>(values, UnlockAll);
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
            finally
            {
                if (lockedCount != count)
                {
                    for (var u = 0; u < count; ++u)
                    {
                        if (values[u] != null)
                        {
                            locks[u].BarelyUnlock();
                        }
                    }
                }
            }
        }
    }
}
