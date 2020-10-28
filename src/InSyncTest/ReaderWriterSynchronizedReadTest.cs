using InSync;
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
    public class ReaderWriterSynchronizedReadTest : SynchronizedTestBase<ReaderSynchronized<object>, object, SynchronizationLockException>
    {
        private readonly object value = new object();
        private ReaderWriterLockSlim rwLock;

        protected override ReaderSynchronized<object> Create(bool locked)
        {
            if (locked)
            {
                rwLock.EnterReadLock();
            }
            return new ReaderWriterSynchronized<object, object>(rwLock, value, value).Reader;
        }

        protected override void AssertLocked(object value)
        {
            rwLock.IsReadLockHeld.ShouldBeTrue();
            value.ShouldBeSameAs(this.value);
        }

        protected override void AssertNotLocked()
        {
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        protected override ReaderSynchronized<object> CreateNotLockable()
        {
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();
            return new ReaderWriterSynchronized<object, object>(rwLock, value, value).Reader;
        }

        [SetUp]
        public void Setup()
        {
            rwLock = new ReaderWriterLockSlim();
        }

        [Test]
        public void BarelyTryLock_ConcurrentAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new object();
            var subject = new ReaderWriterSynchronized<object, object>(rwLock, value, value).Reader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterReadLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.BarelyTryLock(out var result).ShouldBeTrue();

            result.ShouldBe(value);
            rwLock.IsReadLockHeld.ShouldBeTrue();
        }
        
        [Test]
        public void WithLockAction_Reentrant()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            subject.WithLock((v) =>
            {
                subject.WithLock((v2) =>
                {
                    rwLock.IsReadLockHeld.ShouldBeTrue();
                    v2.ShouldBe(value);
                });
            });

            rwLock.IsReadLockHeld.ShouldBeFalse();
        }
    }
}
