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
    public class ReaderWriterSynchronizedUpgradeableReadTest : SynchronizedTestBase<UpgradeableReaderSynchronized<ReaderWriterSynchronizedUpgradeableReadTest.Read>, ReaderWriterSynchronizedUpgradeableReadTest.Read, SynchronizationLockException>
    {
        public class Write
        {
        }

        public class Read
        {

        }

        private readonly Write wValue = new Write();
        private readonly Read rValue = new Read();
        private ReaderWriterLockSlim rwLock;

        [SetUp]
        public void Setup()
        {
            rwLock = new ReaderWriterLockSlim();
        }

        protected override UpgradeableReaderSynchronized<Read> Create(bool locked)
        {
            if (locked)
            {
                rwLock.EnterUpgradeableReadLock();
            }
            return new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
        }

        protected override UpgradeableReaderSynchronized<Read> CreateNotLockable()
        {
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();
            return new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
        }

        protected override void AssertLocked(Read value)
        {
            rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
            value.ShouldBeSameAs(rValue);
        }

        protected override void AssertNotLocked()
        {
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void BarelyUpgrade()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue);
            subject.UpgradeableReader.BarelyLock();

            // act and assert
            var result = subject.BarelyLock();

            result.ShouldBe(wValue);
        }

        [Test]
        public void BarelyTryLock_ConcurrentAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterReadLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.BarelyTryLock(out var result).ShouldBeTrue();

            result.ShouldBe(rValue);
            rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
        }

        [Test]
        public void BarelyTryUpgrade()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue);
            subject.UpgradeableReader.BarelyLock();

            // act and assert
            var result = subject.BarelyLock();

            result.ShouldBe(wValue);
            rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
        }
        
        [Test]
        public void WithLockActionUpgrade()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue);

            // act and assert
            subject.UpgradeableReader.WithLock((r) =>
            {
                subject.WithLock((w) =>
                {
                    rwLock.IsWriteLockHeld.ShouldBeTrue();
                    w.ShouldBe(wValue);
                });
            });

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Reentrant()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            subject.WithLock((v) =>
            {
                subject.WithLock((v2) =>
                {
                    rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                    v2.ShouldBe(rValue);
                });
            });

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }
        
        [Test]
        public void LockUpgrade()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue);

            // act and assert
            using (var rGuard = subject.UpgradeableReader.Lock())
            using (var wGuard = subject.Lock())
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                wGuard.Value.ShouldBe(wValue);
            }

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void LockTryUpgrade()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue);

            // act and assert
            using (var rGuard = subject.UpgradeableReader.Lock())
            using (var wGuard = subject.TryLock())
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                wGuard.Value.ShouldBe(wValue);
            }

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }
    }
}
