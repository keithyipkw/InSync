using InSync;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace InSyncTest
{
    [TestFixture]
    public abstract class SynchronizedTestBase<Lock, Value, InnerUnlockException>
        where Lock : ISynchronized<Value>, IBareLock<Value>
        where Value : class
    {
        protected abstract Lock Create(bool locked);

        protected virtual Lock CreateNotLockable()
        {
            return Create(true);
        }

        protected virtual Lock CreateNotUnlockable()
        {
            return Create(false);
        }

        protected abstract void AssertLocked(Value value);

        protected abstract void AssertNotLocked();

        [Test]
        public void BarelyLock_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            var result = subject.BarelyLock();

            AssertLocked(result);
        }

        [Test]
        public void BarelyUnlock_Release()
        {
            // setup
            var subject = Create(true);

            // act and assert
            subject.BarelyUnlock();

            AssertNotLocked();
        }

        [Test]
        public void BarelyUnlock_Exception()
        {
            // setup
            var subject = CreateNotUnlockable();

            // act and assert
            var exception = Should.Throw<UnlockException>(() => subject.BarelyUnlock());

            exception.PriorException.ShouldBeNull();
            exception.InnerException.ShouldBeOfType<InnerUnlockException>();
            exception.InnerExceptions.Count.ShouldBe(1);
            exception.InnerExceptions
                .ToDictionary(kv => kv.Key, kv => kv.Value.GetType())
                .ShouldBe(new Dictionary<int, Type>
                {
                    [0] = typeof(InnerUnlockException),
                });
        }

        [Test]
        public void BarelyTryLock_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            subject.BarelyTryLock(out var result).ShouldBeTrue();

            AssertLocked(result);
        }

        [Test]
        public void BarelyTryLock_NotAcquire()
        {
            // setup
            var subject = CreateNotLockable();

            // act and assert
            subject.BarelyTryLock(out var result).ShouldBeFalse();

            result.ShouldBeNull();
        }

        [Test]
        public void WithLockAction_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            subject.WithLock((v) =>
            {
                AssertLocked(v);
            });

            AssertNotLocked();
        }

        [Test]
        public void WithLockAction_ExceptionInAction_Release()
        {
            // setup
            var subject = Create(false);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock((v) =>
            {
                throw new CustomException();
            }));

            AssertNotLocked();
        }

        [Test]
        public void WithLockFunc_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            subject.WithLock((v) =>
            {
                AssertLocked(v);
                return 1;
            }).ShouldBe(1);

            AssertNotLocked();
        }

        [Test]
        public void WithLockFunc_ExceptionInAction_Release()
        {
            // setup
            var subject = Create(false);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock(new Func<Value, int>((v) =>
            {
                throw new CustomException();
            })));

            AssertNotLocked();
        }

        [Test]
        public void TryWithLock_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            subject.TryWithLock((v) =>
            {
                AssertLocked(v);
            }).ShouldBeTrue();

            AssertNotLocked();
        }

        [Test]
        public void TryWithLock_NotAcquire()
        {
            // setup
            var subject = CreateNotLockable();

            // act and assert
            subject.TryWithLock((v) =>
            {
                Assert.Fail();
            }).ShouldBeFalse();
        }

        [Test]
        public void TryWithLockAction_ExceptionInAction_Release()
        {
            // setup
            var subject = Create(false);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Action<Value>((v) =>
            {
                throw new CustomException();
            })));

            AssertNotLocked();
        }

        [Test]
        public void Lock_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            using (var guard = subject.Lock())
            {
                AssertLocked(guard.Value);
            }

            AssertNotLocked();
        }

        [Test]
        public void TryLock_Acquire()
        {
            // setup
            var subject = Create(false);

            // act and assert
            using (var guard = subject.TryLock())
            {
                AssertLocked(guard.Value);
            }

            AssertNotLocked();
        }

        [Test]
        public void TryLock_NotAcquire()
        {
            // setup
            var subject = CreateNotLockable();

            // act and assert
            using (var guard = subject.TryLock())
            {
                guard.ShouldBeNull();
            }
        }
    }
}
