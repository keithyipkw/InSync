using InSync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace InSyncBenchmark
{
    public class Philosopher
    {
        private const int FullTimeMs = 10_000;

        public Philosopher(IEnumerable<IBareLock> locks, Action<IReadOnlyList<IBareLock>, Action> lockToEat)
        {
            this.locks = locks.ToList();
            this.lockToEat = lockToEat;
        }

        private readonly Random random = new Random();
        private int totalEatingTimeMs;
        private Stopwatch stopwatch = new Stopwatch();
        private List<IBareLock> locks;
        private readonly Action<IReadOnlyList<IBareLock>, Action> lockToEat;

        public void Dine()
        {
            while(totalEatingTimeMs < FullTimeMs)
            {
                Eat();
            }
        }

        private void Eat()
        {
            locks.Shuffle(random);
            var time = Math.Min(FullTimeMs - totalEatingTimeMs, random.Next(1, 11));
            lockToEat(locks, () =>
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
