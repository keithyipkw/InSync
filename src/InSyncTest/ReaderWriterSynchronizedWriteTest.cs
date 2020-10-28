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
    public class ReaderWriterSynchronizedWriteTest : SynchronizedTestBase<ReaderWriterSynchronized<object, object>, object, SynchronizationLockException>
    {
        private readonly object value = new object();
        private ReaderWriterLockSlim rwLock;

        [SetUp]
        public void Setup()
        {
            rwLock = new ReaderWriterLockSlim();
        }

        protected override ReaderWriterSynchronized<object, object> Create(bool locked)
        {
            if (locked)
            {
                rwLock.EnterWriteLock();
            }
            return new ReaderWriterSynchronized<object, object>(rwLock, value, value);
        }

        protected override ReaderWriterSynchronized<object, object> CreateNotLockable()
        {
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();
            return new ReaderWriterSynchronized<object, object>(rwLock, value, value);
        }

        protected override void AssertLocked(object value)
        {
            rwLock.IsWriteLockHeld.ShouldBeTrue();
            value.ShouldBeSameAs(this.value);
        }

        protected override void AssertNotLocked()
        {
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Reentrant()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            var value = new object();
            var subject = new ReaderWriterSynchronized<object, object>(rwLock, value, value);

            // act and assert
            subject.WithLock((v) =>
            {
                subject.WithLock((v2) =>
                {
                    rwLock.IsWriteLockHeld.ShouldBeTrue();
                    v2.ShouldBe(value);
                });
            });

            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }
    }
}
