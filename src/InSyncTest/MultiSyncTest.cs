using InSync;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSyncTest
{
    [TestFixture]
    public class MultiSyncTest
    {
        private delegate bool TryLockDelegate(out object value);
        private delegate bool TryLockTimeoutDelegate(int timeout, out object value);

        public static IEnumerable AcquireCases(string name)
        {
            var cases = new int[][][]
            {
                new[] { new[] { 0, 0, 0 } },
                new[] { new[] { 0, 1, 0 } },
                new[] { new[] { 0, 0, 1 } },

                new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 0 } },
                new[] { new[] { 0, 1, 0 }, new[] { 0, 0, 1 } },

                new[] { new[] { 0, 0, 1 }, new[] { 1, 0, 0 } },
                new[] { new[] { 0, 0, 1 }, new[] { 0, 1, 0 } },

                new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 0 }, new[] { 0, 1, 0 } },
                new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 0 }, new[] { 0, 0, 1 } },
            };
            foreach (var data in cases)
            {
                yield return new TestCaseData(new[] { data })
                    .SetName($"{name}({string.Join(", ", data.Select(d => $"[{string.Join(", ", d)}]"))})");
            }
        }

        [TestCaseSource(nameof(AcquireCases), new object[] { nameof(AllBareLock_Acquire) })]
        public void AllBareLock_Acquire(int[][] lockStates)
        {
            AllBareLock_Acquire(lockStates, locks => MultiSync.All(locks));
        }

        public static IEnumerable AcquireWithTimeoutCases(string name)
        {
            var cases = new int[][][]
            {
                new[] { new[] { 0, 0, 0 } },
                new[] { new[] { 0, 1, 0 } },
                new[] { new[] { 0, 0, 1 } },

                new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 0 } },
                new[] { new[] { 0, 1, 0 }, new[] { 0, 0, 1 } },

                new[] { new[] { 0, 0, 1 }, new[] { 1, 0, 0 } },
                new[] { new[] { 0, 0, 1 }, new[] { 0, 1, 0 } },

                new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 0 }, new[] { 0, 1, 0 } },
                new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 0 }, new[] { 0, 0, 1 } },
            };
            var methods = new[]
            {
                TimingMethod.Stopwatch,
                TimingMethod.EnvironmentTick,
                TimingMethod.DateTime
            };
            foreach (var method in methods)
            {
                foreach (var data in cases)
                {
                    yield return new TestCaseData(new object[] { method, data })
                        .SetName($"{name}({method}, {string.Join(", ", data.Select(d => $"[{string.Join(", ", d)}]"))})");
                }
            }
        }

        [TestCaseSource(nameof(AcquireWithTimeoutCases), new object[] { nameof(AllBareLockTimeout_InfiniteTimeout_Acquire) })]
        public void AllBareLockTimeout_InfiniteTimeout_Acquire(TimingMethod timingMethod, int[][] lockStates)
        {
            AllBareLock_Acquire(lockStates, locks => MultiSync.All(locks, -1, timingMethod));
        }

        private void AllBareLock_Acquire(int[][] lockStates, Func<List<IBareLock>, GuardedValue<IReadOnlyList<object>>> func)
        {
            // setup
            var remainingStates = lockStates.ToList();
            var locks = new List<IBareLock>();
            var currentStates = remainingStates.First().ToList();
            remainingStates.RemoveAt(0);
            int count = currentStates.Count;
            for (int i = 0; i < count; ++i)
            {
                int iCopy = i;
                var l = new Mock<IBareLock>(MockBehavior.Strict);
                l.Setup(x => x.BarelyLock()).Returns(() =>
                {
                    currentStates[iCopy].ShouldBe(0);
                    ++currentStates[iCopy];
                    return iCopy.ToString();
                });
                var tryLockDelegate = new TryLockDelegate((out object v) =>
                {
                    if (currentStates[iCopy] > 0)
                    {
                        // unlock "externally"
                        --currentStates[iCopy];
                        var next = remainingStates.FirstOrDefault();
                        if (next != null)
                        {
                            remainingStates.RemoveAt(0);
                            for (int s = 0; s < count; ++s)
                            {
                                currentStates[s] += next[s];
                            }
                        }
                        v = null;
                        return false;
                    }
                    ++currentStates[iCopy];
                    v = iCopy.ToString();
                    return true;
                });
                object valueToken;
                l.Setup(x => x.BarelyTryLock(out valueToken)).Returns(tryLockDelegate);
                var tryLockTimeoutDelegate = new TryLockTimeoutDelegate((int _, out object v) =>
                {
                    return tryLockDelegate(out v);
                });
                l.Setup(x => x.BarelyTryLock(It.IsAny<int>(), out valueToken)).Returns(tryLockTimeoutDelegate);
                l.Setup(x => x.BarelyUnlock()).Callback(() => --currentStates[iCopy]);
                locks.Add(l.Object);
            }

            // act
            var guard = func(locks);

            // assert
            currentStates.ShouldBe(Enumerable.Repeat(1, count));
            guard.Value.ShouldBe(Enumerable.Range(0, count).Select(x => x.ToString()));
        }

        [TestCase(60_000, TimingMethod.Stopwatch)]
        [TestCase(60_000, TimingMethod.EnvironmentTick)]
        [TestCase(60_000, TimingMethod.DateTime)]
        [TestCase(int.MaxValue, TimingMethod.Stopwatch)]
        [TestCase(int.MaxValue, TimingMethod.EnvironmentTick)]
        [TestCase(int.MaxValue, TimingMethod.DateTime)]
        public void AllBareLockTimeout_MaxTimeout_Acquire(int timeoutMs, TimingMethod timingMethod)
        {
            AllBareLock_Acquire(new[] { new[] { 0, 0, 0 } }, locks => MultiSync.All(locks, timeoutMs, timingMethod));
        }

        [TestCase(TimingMethod.Stopwatch, 10)]
        [TestCase(TimingMethod.EnvironmentTick, 100)]
        [TestCase(TimingMethod.DateTime, 100)]
        public void AllBareLockTimeout_Timeout_NotAcquire(TimingMethod timingMethod, int sleepMs)
        {
            // setup
            var locks = new List<IBareLock>();
            object valueToken = null;
            {
                var l = new Mock<IBareLock>(MockBehavior.Strict);
                l.Setup(x => x.BarelyTryLock(1, out valueToken)).Returns(new TryLockTimeoutDelegate((int _, out object v) =>
                {
                    Thread.Sleep(sleepMs);
                    v = new object();
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock());
                locks.Add(l.Object);
            }
            {
                var l = new Mock<IBareLock>(MockBehavior.Strict);
                l.Setup(x => x.BarelyTryLock(out valueToken)).Returns(false);
                l.Setup(x => x.BarelyTryLock(0, out valueToken)).Returns(false);
                l.Setup(x => x.BarelyUnlock());
                locks.Add(l.Object);
            }
            locks.Add(new Mock<IBareLock>(MockBehavior.Strict).Object); // prevent infinite loop

            // act
            var guard = MultiSync.All(locks, 1, timingMethod);

            // assert
            guard.ShouldBeNull();
        }

        [Test]
        public void AllBareLock_Release()
        {
            // setup
            var locks = new List<IBareLock>();
            var currentStates = new List<int> { 0, 0, 0 };
            int count = currentStates.Count;
            for (int i = 0; i < count; ++i)
            {
                int iCopy = i;
                var l = new Mock<IBareLock>(MockBehavior.Strict);
                l.Setup(x => x.BarelyLock()).Returns(() =>
                {
                    ++currentStates[iCopy];
                    return iCopy.ToString();
                });
                object valueToken;
                l.Setup(x => x.BarelyTryLock(out valueToken)).Returns(new TryLockDelegate((out object v) =>
                {
                    ++currentStates[iCopy];
                    v = iCopy.ToString();
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock()).Callback(() => --currentStates[iCopy]);
                locks.Add(l.Object);
            }
            var guard = MultiSync.All(locks);

            // act
            guard.Dispose();

            // assert
            currentStates.ShouldBe(Enumerable.Repeat(0, count));
        }

        [Test]
        public void AllTupleBareLock_Release()
        {
            // setup
            var lock1 = new Mock<IBareLock<List<byte>>>(MockBehavior.Strict);
            var lock2 = new Mock<IBareLock<List<short>>>(MockBehavior.Strict);
            var lock3 = new Mock<IBareLock<List<int>>>(MockBehavior.Strict);
            var lock4 = new Mock<IBareLock<List<long>>>(MockBehavior.Strict);
            var locks = new List<Mock<IBareLock>>
            {
                lock1.As<IBareLock>(),
                lock2.As<IBareLock>(),
                lock3.As<IBareLock>(),
                lock4.As<IBareLock>(),
            };
            var types = new List<Type> { typeof(List<byte>), typeof(List<short>), typeof(List<int>), typeof(List<long>) };
            var currentStates = new List<int> { 0, 0, 0, 0 };
            int count = currentStates.Count;
            for (int i = 0; i < count; ++i)
            {
                int iCopy = i;
                var l = locks[i];
                l.Setup(x => x.BarelyLock()).Returns(() =>
                {
                    ++currentStates[iCopy];
                    return Activator.CreateInstance(types[iCopy]);
                });
                object valueToken;
                l.Setup(x => x.BarelyTryLock(out valueToken)).Returns(new TryLockDelegate((out object v) =>
                {
                    ++currentStates[iCopy];
                    v = Activator.CreateInstance(types[iCopy]);
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock()).Callback(() => --currentStates[iCopy]);
            }
            var guard = MultiSync.All(lock1.Object, lock2.Object, lock3.Object, lock4.Object);

            // act
            guard.Dispose();

            // assert
            currentStates.ShouldBe(Enumerable.Repeat(0, count));
        }
        
        [Test]
        public void AllBareLock_ExceptionInRelease_ThrowsUnlockException()
        {
            // setup
            var locks = new List<IBareLock<string>>();
            var l = new Mock<IBareLock<string>>(MockBehavior.Strict);
            l.As<IBareLock>().Setup(x => x.BarelyLock()).Returns(() =>
            {
                return "0";
            });
            l.Setup(x => x.BarelyUnlock()).Throws(new SynchronizationLockException());
            locks.Add(l.Object);
            var guard = MultiSync.All(locks);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => guard.Dispose());
            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(SynchronizationLockException)
                });
        }

        [Test]
        public void AllBareLock_ExceptionInAcquireAndRelease_ThrowsUnlockException()
        {
            // setup
            var locks = new List<IBareLock<string>>();
            {
                var l = new Mock<IBareLock<string>>(MockBehavior.Strict);
                l.As<IBareLock>().Setup(x => x.BarelyLock()).Returns(() =>
                {
                    return "0";
                });
                l.Setup(x => x.BarelyUnlock()).Throws(new SynchronizationLockException());
                locks.Add(l.Object);
            }
            {
                var l = new Mock<IBareLock<string>>(MockBehavior.Strict);
                l.As<IBareLock>().Setup(x => x.BarelyLock()).Returns(() =>
                {
                    return "1";
                });
                object valueToken;
                l.As<IBareLock>().Setup(x => x.BarelyTryLock(out valueToken)).Returns(new TryLockDelegate((out object v) =>
                {
                    v = "1";
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock());
                locks.Add(l.Object);
            }
            var guard = MultiSync.All(locks);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => guard.Dispose());
            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(SynchronizationLockException)
                });
        }

        [TestCaseSource(nameof(AcquireCases), new object[] { nameof(AllBareAsyncLock_Acquire) })]
        public async Task AllBareAsyncLock_Acquire(int[][] initialLockStates)
        {
            // setup
            var remainingStates = initialLockStates.ToList();
            var locks = new List<IBareAsyncLock>();
            var currentStates = remainingStates.First().ToList();
            remainingStates.RemoveAt(0);
            int count = currentStates.Count;
            for (int i = 0; i < count; ++i)
            {
                int iCopy = i;
                var l = new Mock<IBareAsyncLock>(MockBehavior.Strict);
                l.Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
                {
                    currentStates[iCopy].ShouldBe(0);
                    ++currentStates[iCopy];
                    return iCopy.ToString();
                });
                object valueToken;
                l.Setup(x => x.BarelyTryLock(out valueToken)).Returns(new TryLockDelegate((out object v) =>
                {
                    if (currentStates[iCopy] > 0)
                    {
                        // unlock "externally"
                        --currentStates[iCopy];
                        var next = remainingStates.FirstOrDefault();
                        if (next != null)
                        {
                            remainingStates.RemoveAt(0);
                            for (int s = 0; s < count; ++s)
                            {
                                currentStates[s] += next[s];
                            }
                        }
                        v = null;
                        return false;
                    }
                    ++currentStates[iCopy];
                    v = iCopy.ToString();
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock()).Callback(() => --currentStates[iCopy]);
                locks.Add(l.Object);
            }

            // act
            var guard = await MultiSync.AllAsync(locks);

            // assert
            currentStates.ShouldBe(Enumerable.Repeat(1, count));
            guard.Value.ShouldBe(Enumerable.Range(0, count).Select(x => x.ToString()));
        }

        [Test]
        public async Task AllBareAsyncLock_Release()
        {
            // setup
            var locks = new List<IBareAsyncLock>();
            var currentStates = new List<int> { 0, 0, 0 };
            int count = currentStates.Count;
            for (int i = 0; i < count; ++i)
            {
                int iCopy = i;
                var l = new Mock<IBareAsyncLock>(MockBehavior.Strict);
                l.Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
                {
                    ++currentStates[iCopy];
                    return iCopy.ToString();
                });
                object valueToken;
                l.Setup(x => x.BarelyTryLock(out valueToken)).Returns(new TryLockDelegate((out object v) =>
                {
                    ++currentStates[iCopy];
                    v = iCopy.ToString();
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock()).Callback(() => --currentStates[iCopy]);
                locks.Add(l.Object);
            }
            var guard = await MultiSync.AllAsync(locks);

            // act
            guard.Dispose();

            // assert
            currentStates.ShouldBe(Enumerable.Repeat(0, count));
        }

        [Test]
        public async Task AllBareAsyncLock_ExceptionInRelease_ThrowsUnlockException()
        {
            // setup
            var locks = new List<IBareAsyncLock<string>>();
            var l = new Mock<IBareAsyncLock<string>>(MockBehavior.Strict);
            l.As<IBareAsyncLock>().Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            {
                return "0";
            });
            l.Setup(x => x.BarelyUnlock()).Throws(new SynchronizationLockException());
            locks.Add(l.Object);
            var guard = await MultiSync.AllAsync(locks);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => guard.Dispose());
            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(SynchronizationLockException)
                });
        }

        [Test]
        public async Task AllBareAsyncLock_ExceptionInAcquireAndRelease_ThrowsUnlockException()
        {
            // setup
            var locks = new List<IBareAsyncLock<string>>();
            {
                var l = new Mock<IBareAsyncLock<string>>(MockBehavior.Strict);
                l.As<IBareAsyncLock>().Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
                {
                    return "0";
                });
                l.Setup(x => x.BarelyUnlock()).Throws(new SynchronizationLockException());
                locks.Add(l.Object);
            }
            {
                var l = new Mock<IBareAsyncLock<string>>(MockBehavior.Strict);
                l.As<IBareAsyncLock>().Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
                {
                    return "1";
                });
                object valueToken;
                l.As<IBareAsyncLock>().Setup(x => x.BarelyTryLock(out valueToken)).Returns(new TryLockDelegate((out object v) =>
                {
                    v = "1";
                    return true;
                }));
                l.Setup(x => x.BarelyUnlock());
                locks.Add(l.Object);
            }
            var guard = await MultiSync.AllAsync(locks);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => guard.Dispose());
            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(SynchronizationLockException)
                });
        }
    }
}
