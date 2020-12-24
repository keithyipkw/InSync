using InSync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSyncBenchmark
{
    public class AsyncPhilosopher
    {
        private const int FullTimeMs = 10_000;

        public AsyncPhilosopher(IEnumerable<IBareAsyncLock> locks, Func<IReadOnlyList<IBareAsyncLock>, Action, Task> lockToEat)
        {
            this.locks = locks.ToList();
            this.lockToEat = lockToEat;
        }

        private readonly Random random = new Random();
        private int totalEatingTimeMs;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly List<IBareAsyncLock> locks;
        private readonly Func<IReadOnlyList<IBareAsyncLock>, Action, Task> lockToEat;

        public async Task DineAsync()
        {
            while(totalEatingTimeMs < FullTimeMs)
            {
                await EatAsync();
            }
        }

        private async Task EatAsync()
        {
            locks.Shuffle(random);
            var time = Math.Min(FullTimeMs - totalEatingTimeMs, random.Next(1, 11));
            await lockToEat(locks, () =>
            {
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < time)
                {
                }
                stopwatch.Stop();
            });
            totalEatingTimeMs += time;
        }
    }
}
