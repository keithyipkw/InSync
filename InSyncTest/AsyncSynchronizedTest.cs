using InSync;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSyncTest
{
    [TestFixture]
    public class AsyncSynchronizedTest
    {
        [Test]
        public void BarelyLock_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var result = subject.BarelyLock();

            semaphore.CurrentCount.ShouldBe(0);
            result.ShouldBe(value);
        }
        
        [Test]
        public void BarelyUnlock_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(0);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.BarelyUnlock();

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void BarelyTryLock_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.BarelyTryLock(out List result).ShouldBeTrue();

            semaphore.CurrentCount.ShouldBe(0);
            result.ShouldBe(value);
        }
        
        [Test]
        public void BarelyTryLock_NotAcquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(0);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);
            var setupEvent = new AutoResetEvent(false);
            var cleanUpEvent = new AutoResetEvent(false);
            
            // act and assert
            subject.BarelyTryLock(out List result).ShouldBeFalse();

            semaphore.CurrentCount.ShouldBe(0);
            result.ShouldBeNull();
        }

        [Test]
        public void WithLockAction_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.WithLock((v) =>
            {
                semaphore.CurrentCount.ShouldBe(0);
                v.ShouldBe(value);
            });

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void WithLockAction_Exception_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);
            
            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock((v) =>
            {
                throw new CustomException();
            }));

            semaphore.CurrentCount.ShouldBe(1);
        }
        
        [Test]
        public void WithLockFunc_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.WithLock((v) =>
            {
                semaphore.CurrentCount.ShouldBe(0);
                v.ShouldBe(value);
                return 1;
            }).ShouldBe(1);
            
            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void WithLockFunc_Exception_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock(new Func<List, int>((v) =>
            {
                throw new CustomException();
            })));
            
            semaphore.CurrentCount.ShouldBe(1);
        }
        
        [Test]
        public void TryWithLock_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                semaphore.CurrentCount.ShouldBe(0);
                v.ShouldBe(value);
            }).ShouldBeTrue();
            
            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void TryWithLock_NotAcquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(0);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                Assert.Fail();
            }).ShouldBeFalse();
            
            semaphore.CurrentCount.ShouldBe(0);
        }

        [Test]
        public void TryWithLock_Exception_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Action<List>((v) =>
            {
                throw new CustomException();
            })));

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void TryWithLockFunc_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                semaphore.CurrentCount.ShouldBe(0);
                v.ShouldBe(value);
                return 1;
            }, out var result).ShouldBeTrue();

            result.ShouldBe(1);
            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void TryWithLockFunc_NotAcquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(0);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);
            
            // act and assert
            subject.TryWithLock((v) =>
            {
                Assert.Fail();
                return 1;
            }, out var result).ShouldBeFalse();

            result.ShouldBe(0);
            semaphore.CurrentCount.ShouldBe(0);
        }

        [Test]
        public void TryWithLockFunc_Exception_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Func<List, int>((v) =>
            {
                throw new CustomException();
            }), out var result));

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void Lock_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            using (var guard = subject.Lock())
            {
                semaphore.CurrentCount.ShouldBe(0);
                guard.Value.ShouldBe(value);
            }

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void TryLock_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            using (var guard = subject.TryLock())
            {
                semaphore.CurrentCount.ShouldBe(0);
                guard.Value.ShouldBe(value);
            }

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void TryLock_NotAcquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(0);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);
            var setupEvent = new AutoResetEvent(false);
            var cleanUpEvent = new AutoResetEvent(false);

            // act and assert
            using (var guard = subject.TryLock())
            {
                semaphore.CurrentCount.ShouldBe(0);
                guard.ShouldBeNull();
            }

            semaphore.CurrentCount.ShouldBe(0);
        }

        [Test]
        public async Task BarelyLockAsync_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var result = await subject.BarelyLockAsync();

            semaphore.CurrentCount.ShouldBe(0);
            result.ShouldBe(value);
        }

        [Test]
        public void CancelBarelyLockAsync_Cancel()
        {
            // setup
            var semaphore = new SemaphoreSlim(0);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<OperationCanceledException>(() => subject.BarelyLockAsync(new CancellationTokenSource(0).Token));
        }

        [Test]
        public async Task WithLockAsyncAction_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            await subject.WithLockAsync((v) =>
            {
                semaphore.CurrentCount.ShouldBe(0);
                v.ShouldBe(value);
            });

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void CancelWithLockAsyncAction_Cancel()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<TaskCanceledException>(() => subject.WithLockAsync((v) => Task.FromResult(0), new CancellationTokenSource(0).Token));

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void WithLockAsyncActionException_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLockAsync((v) =>
            {
                throw new CustomException();
            }));

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public async Task WithLockFuncAsync_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            (await subject.WithLockAsync((v) =>
            {
                semaphore.CurrentCount.ShouldBe(0);
                v.ShouldBe(value);
                return 1;
            })).ShouldBe(1);

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void WithLockAsyncFunc_Exception_Release()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLockAsync(new Func<List, int>((v) =>
            {
                throw new CustomException();
            })));

            semaphore.CurrentCount.ShouldBe(1);
        }
        
        [Test]
        public async Task LockAsync_Acquire()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            using (var guard = await subject.LockAsync())
            {
                semaphore.CurrentCount.ShouldBe(0);
                guard.Value.ShouldBe(value);
            }

            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void CancelLockAsync_Cancel()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            Should.Throw<TaskCanceledException>(() => subject.LockAsync(new CancellationTokenSource(0).Token));
        }
    }
}
