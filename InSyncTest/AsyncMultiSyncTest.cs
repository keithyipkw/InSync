using InSync;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSyncTest
{
    [TestFixture]
    public class AsyncMultiSyncTest
    {
        private delegate bool TryLockDelegate(out object value);
        
        [TestCase(new[] { 0, 0, 0 })]
        [TestCase(new[] { 0, 1, 0 })]
        [TestCase(new[] { 0, 0, 1 })]

        [TestCase(new[] { 0, 1, 0 }, new[] { 1, 0, 0 })]
        [TestCase(new[] { 0, 1, 0 }, new[] { 0, 0, 1 })]

        [TestCase(new[] { 0, 0, 1 }, new[] { 1, 0, 0 })]
        [TestCase(new[] { 0, 0, 1 }, new[] { 0, 1, 0 })]

        [TestCase(new[] { 0, 1, 0 }, new[] { 1, 0, 0 }, new[] { 0, 1, 0 })]
        [TestCase(new[] { 0, 1, 0 }, new[] { 1, 0, 0 }, new[] { 0, 0, 1 })]
        public async Task AllBareLock_Acquire(params int[][] initialLockStates)
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
            guard.ShouldBe(Enumerable.Range(0, count).Select(x => x.ToString()));
        }

        [Test]
        public async Task AllBareLock_Release()
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
                    currentStates[iCopy].ShouldBe(0);
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
        public async Task AllBareLock_ExceptionInRelease_ThrowsUnlockException()
        {
            // setup
            var locks = new List<IBareAsyncLock<string>>();
            var l = new Mock<IBareAsyncLock<string>>(MockBehavior.Strict);
            l.As<IBareAsyncLock>().Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    return "0";
                });
            l.Setup(x => x.BarelyUnlock()).Throws(new SynchronizationLockException());
            locks.Add(l.Object);
            var guard = await MultiSync.AllAsync(locks);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => guard.Dispose());
            exception.InnerException.ShouldBeNull();
            exception.ExceptionsFromUnlocking.Select(e => e.GetType()).ShouldBe(new[] { typeof(SynchronizationLockException) });
        }

        [Test]
        public async Task AllBareLock_ExceptionInAcquireAndRelease_ThrowsUnlockException()
        {
            // setup
            var locks = new List<IBareAsyncLock<string>>();
            {
                var l = new Mock<IBareAsyncLock<string>>(MockBehavior.Strict);
                l.As<IBareAsyncLock>().Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() =>
                    {
                        return "0";
                    });
                l.Setup(x => x.BarelyUnlock()).Throws(new SynchronizationLockException());
                locks.Add(l.Object);
            }
            {
                var l = new Mock<IBareAsyncLock<string>>(MockBehavior.Strict);
                l.As<IBareAsyncLock>().Setup(x => x.BarelyLockAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() =>
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
            exception.InnerException.ShouldBeNull();
            exception.ExceptionsFromUnlocking.Select(e => e.GetType()).ShouldBe(new[] { typeof(SynchronizationLockException) });
        }
    }
}
