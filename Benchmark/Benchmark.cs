using InSync;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace InSyncBenchmark
{
    public class Benchmark
    {
        public async Task Run()
        {
            Console.WriteLine("0: single lock");
            Console.WriteLine("1: multiple locks");
            Console.Write("Select one: ");
            int which;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out which))
                {
                    if (which < 2)
                    {
                        break;
                    }
                }
                Console.Write("Select one: ");
            }
            Console.WriteLine();
            switch (which)
            {
                case 0:
                    await BenchmarkSingleLock();
                    break;
                case 1:
                    Dining();
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Done");
            while (true)
            {
                Console.Read();
            }
        }

        private async Task BenchmarkSingleLock()
        {
            var repeat = 300_000_000;

            var watch = new Stopwatch();
            var x = new X();
            var sqrt = (int)Math.Sqrt(1);
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                x.Value += sqrt;
            }
            watch.Stop();
            Console.WriteLine($"Baseline loop {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            await TestSynchronizedAsync(repeat);
            await TestSynchronizeMultipleAsync(repeat);
        }
        
        public async Task TestSynchronizedAsync(int repeat)
        {
            var watch = new Stopwatch();

            var x = new X();
            var sqrt = (int)Math.Sqrt(1);

            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                lock (x)
                {
                    x.Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"Inline lock {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            var syncMonitor = Synchronized.Create(x);
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                syncMonitor.WithLock((v) => v.Value += sqrt);
            }
            watch.Stop();
            Console.WriteLine($"Synchronized.WithLock {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = syncMonitor.Lock())
                {
                    guard.Value.Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"Synchronized.Lock {((double)watch.ElapsedTicks * 100 / repeat)}ns");
            
            Console.WriteLine();

            var syncSemaphore = AsyncSynchronized.Create(x);
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                syncSemaphore.WithLock((v) => v.Value += sqrt);
            }
            watch.Stop();
            Console.WriteLine($"AsyncSynchronized.WithLock {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = syncSemaphore.Lock())
                {
                    guard.Value.Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"AsyncSynchronized.Lock {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                await syncSemaphore.WithLockAsync((v) => v.Value += sqrt);
            }
            watch.Stop();
            Console.WriteLine($"AsyncSynchronized.WithLockAsync {((double)watch.ElapsedTicks * 100 / repeat)}ns");
            
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = await syncSemaphore.LockAsync())
                {
                    guard.Value.Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"AsyncSynchronized.LockAsync {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            Console.WriteLine();
        }

        public async Task TestSynchronizeMultipleAsync(int repeat)
        {
            var watch = new Stopwatch();

            var x1 = new X();
            var x2 = new X();
            var x3 = new X();
            var sqrt = (int)Math.Sqrt(1);
            
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                lock (x1)
                {
                    lock (x2)
                    {
                        x1.Value += sqrt;
                        x2.Value += sqrt;
                    }
                }
            }
            watch.Stop();
            Console.WriteLine($"Inline lock {((double)watch.ElapsedTicks * 100 / repeat)}ns");
            
            var syncMonitor1 = Synchronized.Create(x1);
            var syncMonitor2 = Synchronized.Create(x2);

            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var v1 = syncMonitor1.Lock())
                using (var v2 = syncMonitor2.Lock())
                {
                    v1.Value.Value += sqrt;
                    v2.Value.Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"Synchronized.Lock {((double)watch.ElapsedTicks * 100 / repeat)}ns");
            
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = MultiSync.All(new[] { syncMonitor1, syncMonitor2 }))
                {
                    var values = guard.Value;
                    ((X)values[0]).Value += sqrt;
                    ((X)values[1]).Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"SynchronizeAll.All monitor {((double)watch.ElapsedTicks * 100 / repeat)}ns");
            
            watch.Restart();
            var syncMonitors = new[] { syncMonitor1, syncMonitor2 };
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = MultiSync.All(syncMonitors))
                {
                    var values = guard.Value;
                    ((X)values[0]).Value += sqrt;
                    ((X)values[1]).Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"SynchronizeAll.All(reuse array) monitor {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            watch.Restart();
            var syncSemaphore1 = AsyncSynchronized.Create(x1);
            var syncSemaphore2 = AsyncSynchronized.Create(x2);
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = await MultiSync.AllAsync(new[] { syncSemaphore1, syncSemaphore2 }))
                {
                    var values = guard.Value;
                    ((X)values[0]).Value += sqrt;
                    ((X)values[1]).Value += sqrt;
                }
            }
            watch.Stop();
            Console.WriteLine($"SynchronizeAll.AllAsync semaphoreslim {((double)watch.ElapsedTicks * 100 / repeat)}ns");

            Console.WriteLine();
        }
        
        private class X
        {
            public int Value;
        }

        private void Dining()
        {
            for (var nt = 2; nt <= 32; ++nt)
            {
                for (var method = (Method)0; method < Method.Count; ++method)
                {
                    var table = new List<IBareLock>();
                    var lockOrders = new Dictionary<IBareLock, int>();
                    for (var i = 0; i < nt; ++i)
                    {
                        table.Add(Synchronized.Create(new object()));
                        lockOrders[table[i]] = i;
                    }
                    
                    var diners = new List<Philosopher>();
                    for (var i = 0; i < nt; ++i)
                    {
                        int j = i;
                        int k = j < nt - 1 ? j + 1 : 0;
                        Action<IReadOnlyList<IBareLock>, Action> lockToEat = null;
                        switch (method)
                        {
                            case Method.Order:
                                lockToEat = (locks, eat) =>
                                {
                                    locks = locks.OrderBy(x => lockOrders[x]).ToList();
                                    foreach (var l in locks)
                                    {
                                        l.BarelyLock();
                                    }
                                    eat();
                                    foreach (var l in locks)
                                    {
                                        l.BarelyUnlock();
                                    }
                                };
                                break;
                            case Method.SmartAndPolite:
                                lockToEat = (locks, eat) =>
                                {
                                    using (MultiSync.All(locks))
                                    {
                                        eat();
                                    }
                                };
                                break;
                        }
                        diners.Add(new Philosopher(new[] { table[j], table[k] }, lockToEat));
                    }
                    var threads = new List<Thread>();
                    var stopwatch = new Stopwatch();
                    for (var i = 0; i < nt; ++i)
                    {
                        threads.Add(new Thread(diners[i].Dine));
                    }
                    stopwatch.Start();
                    for (var i = 0; i < nt; ++i)
                    {
                        threads[i].Start();
                    }
                    foreach (var t in threads)
                    {
                        t.Join();
                    }
                    stopwatch.Stop();
                    Console.WriteLine($"{method} {nt} {stopwatch.Elapsed.TotalSeconds}");
                }
            }
        }

        private enum Method
        {
            Order,
            SmartAndPolite,
            Count,
        }
    }
}
