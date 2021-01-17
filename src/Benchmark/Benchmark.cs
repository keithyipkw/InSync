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
        private const int Repeat = 100 * 1000 * 2;

        public async Task Run()
        {
            Console.Error.WriteLine("0: single lock overhead");
            Console.Error.WriteLine("1: multiple lock overhead");
            Console.Error.WriteLine("2: dining philosopher");
            Console.Error.WriteLine("3: dining async philosopher");
            int test;
            int repeat;
            while (true)
            {
                Console.Error.Write("test repeat <core affinity> <core affinity> ...: ");

                var parameters = new List<int>();
                foreach (var p in Console.ReadLine().Split(' '))
                {
                    if (!int.TryParse(p, out var v))
                    {
                        parameters = null;
                        break;
                    }
                    parameters.Add(v);
                }
                if (parameters == null
                    || parameters.Count < 2)
                {
                    continue;
                }

                test = parameters[0];
                repeat = parameters[1];

                var affinity = 0ul;
                foreach (var p in parameters.Skip(2))
                {
                    affinity |= 1ul << p;
                }
                using (var process = Process.GetCurrentProcess())
                {
                    if (affinity > 0)
                    {
                        process.ProcessorAffinity = (IntPtr)affinity;
                    }
                    process.PriorityClass = ProcessPriorityClass.High;
                }

                break;
            }
            Console.Error.WriteLine();
            for (int i = 0; i < repeat; ++i)
            {
                switch (test)
                {
                    case 0:
                        await BenchmarkSingleLock();
                        break;
                    case 1:
                        await BenchmarkMultipleLockAsync();
                        break;
                    case 2:
                        Dining();
                        break;
                    case 3:
                        await DiningAsync();
                        break;
                }
            }
        }

        private async Task BenchmarkSingleLock()
        {
            var watch = new Stopwatch();
            var x = new X();
            var sqrt = (int)Math.Sqrt(1);
            ForceGC();
            watch.Restart();
            for (int i = 0; i < Repeat; ++i)
            {
                x.Value += sqrt;
            }
            watch.Stop();
            Console.WriteLine($"Loop overhead,{((double)watch.ElapsedTicks * 100 / Repeat)}");

            await TestSynchronizedAsync(1, false);
            await TestSynchronizedAsync(Repeat, true);
        }

        public async Task TestSynchronizedAsync(int repeat, bool output)
        {
            var watch = new Stopwatch();

            var x = new X();
            var sqrt = (int)Math.Sqrt(1);

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                lock (x)
                {
                    x.Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"lock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            var syncMonitor = Synchronized.Create(x);
            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                syncMonitor.WithLock((v) => v.Value += sqrt);
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"Synchronized.WithLock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = syncMonitor.Lock())
                {
                    guard.Value.Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"Synchronized.Lock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            var semaphore = new SemaphoreSlim(1);
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                semaphore.Wait();
                x.Value += sqrt;
                semaphore.Release();
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"SemaphoreSlim.Wait,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            var syncSemaphore = AsyncSynchronized.Create(x);
            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                syncSemaphore.WithLock((v) => v.Value += sqrt);
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"AsyncSynchronized.WithLock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = syncSemaphore.Lock())
                {
                    guard.Value.Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"AsyncSynchronized.Lock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                x.Value += sqrt;
                semaphore.Release();
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"SemaphoreSlim.WaitAsync,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                await syncSemaphore.WithLockAsync((v) => v.Value += sqrt);
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"AsyncSynchronized.WithLockAsync,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = await syncSemaphore.LockAsync())
                {
                    guard.Value.Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"AsyncSynchronized.LockAsync,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }
        }

        private async Task BenchmarkMultipleLockAsync()
        {
            var watch = new Stopwatch();
            var x1 = new X();
            var x2 = new X();
            var sqrt = (int)Math.Sqrt(1);
            ForceGC();
            watch.Restart();
            for (int i = 0; i < Repeat; ++i)
            {
                x1.Value += sqrt;
                x2.Value += sqrt;
            }
            watch.Stop();
            Console.WriteLine($"Loop overhead,{((double)watch.ElapsedTicks * 100 / Repeat)}");

            await TestSynchronizeMultipleAsync(1, false);
            await TestSynchronizeMultipleAsync(Repeat, true);
        }

        private async Task TestSynchronizeMultipleAsync(int repeat, bool output)
        {
            var watch = new Stopwatch();

            var x1 = new X();
            var x2 = new X();
            var sqrt = (int)Math.Sqrt(1);

            ForceGC();
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
            if (output)
            {
                Console.WriteLine($"lock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }
            
            var syncMonitor1 = Synchronized.Create(x1);
            var syncMonitor2 = Synchronized.Create(x2);

            ForceGC();
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
            if (output)
            {
                Console.WriteLine($"Synchronized.Lock,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = MultiSync.All(new[] { syncMonitor1, syncMonitor2 }))
                {
                    guard.Value[0].Value += sqrt;
                    guard.Value[1].Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"MultiSync.All Monitor,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            var syncMonitors = new[] { syncMonitor1, syncMonitor2 };
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = MultiSync.All(syncMonitors))
                {
                    guard.Value[0].Value += sqrt;
                    guard.Value[1].Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"MultiSync.All Monitor reusing array,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            watch.Restart();
            var semaphore1 = new SemaphoreSlim(1);
            var semaphore2 = new SemaphoreSlim(1);
            for (int i = 0; i < repeat; ++i)
            {
                await semaphore1.WaitAsync().ConfigureAwait(false);
                await semaphore2.WaitAsync().ConfigureAwait(false);
                x1.Value += sqrt;
                x2.Value += sqrt;
                semaphore2.Release();
                semaphore1.Release();
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"SemaphoreSlim.WaitAsync,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }

            ForceGC();
            var syncSemaphore1 = AsyncSynchronized.Create(x1);
            var syncSemaphore2 = AsyncSynchronized.Create(x2);
            watch.Restart();
            for (int i = 0; i < repeat; ++i)
            {
                using (var guard = await MultiSync.AllAsync(new[] { syncSemaphore1, syncSemaphore2 }))
                {
                    guard.Value[0].Value += sqrt;
                    guard.Value[1].Value += sqrt;
                }
            }
            watch.Stop();
            if (output)
            {
                Console.WriteLine($"MultiSync.AllAsync SemaphoreSlim,{((double)watch.ElapsedTicks * 100 / repeat)}");
            }
        }

        private void ForceGC()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        private class X
        {
            public int Value;
        }

        private enum Method
        {
            Ordered,
            SmartAndPolite,
            Count,
        }

        private void Dining()
        {
            for (var method = (Method)0; method < Method.Count; ++method)
            {
                for (var nt = 2; nt <= 32; ++nt)
                {
                    ForceGC();
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
                            case Method.Ordered:
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
                    Console.WriteLine($"{method},{nt},{stopwatch.Elapsed.TotalSeconds}");
                }
            }
        }

        private async Task DiningAsync()
        {
            for (var method = (Method)0; method < Method.Count; ++method)
            {
                for (var nt = 2; nt <= 32; ++nt)
                {
                    ForceGC();
                    var table = new List<IBareAsyncLock>();
                    var lockOrders = new Dictionary<IBareAsyncLock, int>();
                    for (var i = 0; i < nt; ++i)
                    {
                        table.Add(AsyncSynchronized.Create(new object()));
                        lockOrders[table[i]] = i;
                    }

                    var diners = new List<AsyncPhilosopher>();
                    for (var i = 0; i < nt; ++i)
                    {
                        int j = i;
                        int k = j < nt - 1 ? j + 1 : 0;
                        Func<IReadOnlyList<IBareAsyncLock>, Action, Task> lockToEat = null;
                        switch (method)
                        {
                            case Method.Ordered:
                                lockToEat = async (locks, eat) =>
                                {
                                    locks = locks.OrderBy(x => lockOrders[x]).ToList();
                                    foreach (var l in locks)
                                    {
                                        await l.BarelyLockAsync();
                                    }
                                    eat();
                                    foreach (var l in locks)
                                    {
                                        l.BarelyUnlock();
                                    }
                                };
                                break;
                            case Method.SmartAndPolite:
                                lockToEat = async (locks, eat) =>
                                {
                                    using (await MultiSync.AllAsync(locks))
                                    {
                                        eat();
                                    }
                                };
                                break;
                        }
                        diners.Add(new AsyncPhilosopher(new[] { table[j], table[k] }, lockToEat));
                    }
                    var stopwatch = new Stopwatch();
                    var tasks = new List<Task>();
                    stopwatch.Start();
                    for (var i = 0; i < nt; ++i)
                    {
                        tasks.Add(Task.Run(diners[i].DineAsync));
                    }
                    await Task.WhenAll(tasks);
                    stopwatch.Stop();
                    Console.WriteLine($"{method},{nt},{stopwatch.Elapsed.TotalSeconds}");
                }
            }
        }
    }
}
