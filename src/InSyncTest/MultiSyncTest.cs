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
            var guard = MultiSync.All(locks);

            // assert
            currentStates.ShouldBe(Enumerable.Repeat(1, count));
            guard.Value.ShouldBe(Enumerable.Range(0, count).Select(x => x.ToString()));
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
