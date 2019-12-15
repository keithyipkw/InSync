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
    public class ReaderWriterSynchronizedUpgradeableReadTest
    {
        private class Write
        {
        }

        private class Read
        {

        }

        [Test]
        public void BarelyLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            var result = subject.BarelyLock();

            result.ShouldBe(rValue);
            rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
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
        public void BarelyUnlock_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            rwLock.EnterUpgradeableReadLock();

            // act and assert
            subject.BarelyUnlock();
            
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void BarelyTryLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            subject.BarelyTryLock(out Read result).ShouldBeTrue();

            result.ShouldBe(rValue);
            rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
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
        public void BarelyTryLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.BarelyTryLock(out Read result).ShouldBeFalse();

            result.ShouldBeNull();
        }

        [Test]
        public void WithLockAction_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            subject.WithLock((v) =>
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                v.ShouldBe(rValue);
            });
            
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
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
        public void WithLockAction_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            
            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock((v) =>
            {
                throw new CustomException();
            }));

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
        public void WithLockFunc_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            subject.WithLock((v) =>
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                v.ShouldBe(rValue);
                return 1;
            }).ShouldBe(1);
            
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockFunc_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock(new Func<Read, int>((v) =>
            {
                throw new CustomException();
            })));
            
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }
        
        [Test]
        public void TryWithLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            subject.TryWithLock((v) =>
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                v.ShouldBe(rValue);
            }).ShouldBeTrue();
            
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }
        
        [Test]
        public void TryWithLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.TryWithLock((v) =>
            {
                Assert.Fail();
            }).ShouldBeFalse();
            
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLock_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Action<Read>((v) =>
            {
                throw new CustomException();
            })));

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            subject.TryWithLock((v) =>
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                v.ShouldBe(rValue);
                return 1;
            }, out var result).ShouldBeTrue();

            result.ShouldBe(1);
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.TryWithLock((v) =>
            {
                Assert.Fail();
                return 1;
            }, out var result).ShouldBeFalse();

            result.ShouldBe(0);
            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Func<Read, int>((v) =>
            {
                throw new CustomException();
            }), out var result));

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void Lock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            using (var guard = subject.Lock())
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                guard.Value.ShouldBe(rValue);
            }

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

        [Test]
        public void TryLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;

            // act and assert
            using (var guard = subject.TryLock())
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeTrue();
                guard.Value.ShouldBe(rValue);
            }

            rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var rValue = new Read();
            var wValue = new Write();
            var subject = new ReaderWriterSynchronized<Write, Read>(rwLock, wValue, rValue).UpgradeableReader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            using (var guard = subject.TryLock())
            {
                rwLock.IsUpgradeableReadLockHeld.ShouldBeFalse();
                guard.ShouldBeNull();
            }
        }
    }
}
