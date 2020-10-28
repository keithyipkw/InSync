using InSync;
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
    public class AsyncSynchronizedTest : SynchronizedTestBase<AsyncSynchronized<List>, List, ObjectDisposedException>
    {
        private readonly List value = new List();
        private SemaphoreSlim semaphore;

        [SetUp]
        public void Setup()
        {
            semaphore = new SemaphoreSlim(1);
        }
        
        protected override AsyncSynchronized<List> Create(bool locked)
        {
            if (locked)
            {
                semaphore.Wait();
            }
            return new AsyncSynchronized<List>(semaphore, value);
        }

        protected override AsyncSynchronized<List> CreateNotUnlockable()
        {
            semaphore.Dispose();
            return new AsyncSynchronized<List>(semaphore, value);
        }

        protected override void AssertLocked(List value)
        {
            semaphore.CurrentCount.ShouldBe(0);
            this.value.ShouldBe(value);
        }

        protected override void AssertNotLocked()
        {
            semaphore.CurrentCount.ShouldBe(1);
        }

        [Test]
        public void WithLockAction_ExceptionInAcquire_ThrowLockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);
            semaphore.Dispose();

            // act and assert
            var exception = Should.Throw<LockException>(() => subject.WithLock((v) =>
            {
            }));

            exception.InnerException.ShouldBeOfType<ObjectDisposedException>();
        }

        [Test]
        public void WithLockAction_ExceptionInRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLock((v) =>
            {
                semaphore.Dispose();
            }));

            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }

        [Test]
        public void WithLockAction_ExceptionInActionAndRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLock((v) =>
            {
                semaphore.Dispose();
                throw new SynchronizationLockException();
            }));

            exception.PriorException.ShouldBeOfType<SynchronizationLockException>();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }

        [Test]
        public void WithLockFunc_ExceptionInRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLock(new Func<List, int>((v) =>
            {
                semaphore.Dispose();
                return 0;
            })));

            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }

        [Test]
        public void WithLockFunc_ExceptionInFuncAndRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLock(new Func<List, int>((v) =>
            {
                semaphore.Dispose();
                throw new SynchronizationLockException();
            })));

            exception.PriorException.ShouldBeOfType<SynchronizationLockException>();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }

        [Test]
        public void TryWithLockAction_ExceptionInAcquire_ThrowLockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);
            semaphore.Dispose();

            // act and assert
            var exception = Should.Throw<LockException>(() => subject.TryWithLock((v) =>
            {
            }));

            exception.InnerException.ShouldBeOfType<ObjectDisposedException>();
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
        public void WithLockAsyncAction_Exception_Release()
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
        public void WithLockAsyncAction_ExceptionInRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLockAsync((v) =>
            {
                semaphore.Dispose();
            }));

            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }

        [Test]
        public void WithLockAsyncAction_ExceptionInFuncAndRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLock((v) =>
            {
                semaphore.Dispose();
                throw new SynchronizationLockException();
            }));

            exception.PriorException.ShouldBeOfType<SynchronizationLockException>();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }
        
        [Test]
        public async Task WithLockAsyncFunc_Acquire()
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
        public void WithLockAsyncFunc_ExceptionInRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLockAsync(new Func<List, int>((v) =>
            {
                semaphore.Dispose();
                return 0;
            })));

            exception.PriorException.ShouldBeNull();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
        }

        [Test]
        public void WithLockAsyncFunc_ExceptionInFuncAndRelease_ThrowUnlockException()
        {
            // setup
            var semaphore = new SemaphoreSlim(1);
            var value = new List();
            var subject = new AsyncSynchronized<List>(semaphore, value);

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.WithLockAsync(new Func<List, int>((v) =>
            {
                semaphore.Dispose();
                throw new SynchronizationLockException();
            })));

            exception.PriorException.ShouldBeOfType<SynchronizationLockException>();
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(ObjectDisposedException)
                });
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
