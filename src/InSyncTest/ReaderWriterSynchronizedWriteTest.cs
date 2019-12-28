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
    public class ReaderWriterSynchronizedWriteTest
    {
        [Test]
        public void BarelyLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            var result = subject.BarelyLock();

            result.ShouldBe(value);
            rwLock.IsWriteLockHeld.ShouldBeTrue();
        }

        [Test]
        public void BarelyUnlock_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);
            rwLock.EnterWriteLock();

            // act and assert
            subject.BarelyUnlock();
            
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void BarelyTryLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            subject.BarelyTryLock(out List result).ShouldBeTrue();

            result.ShouldBe(value);
            rwLock.IsWriteLockHeld.ShouldBeTrue();
        }
        
        [Test]
        public void BarelyTryLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.BarelyTryLock(out List result).ShouldBeFalse();

            result.ShouldBeNull();
        }

        [Test]
        public void WithLockAction_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            subject.WithLock((v) =>
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
            });
            
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);
            
            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock((v) =>
            {
                throw new CustomException();
            }));

            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Reentrant()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

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

        [Test]
        public void WithLockFunc_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            subject.WithLock((v) =>
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
                return 1;
            }).ShouldBe(1);
            
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockFunc_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock(new Func<List, int>((v) =>
            {
                throw new CustomException();
            })));
            
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }
        
        [Test]
        public void TryWithLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
            }).ShouldBeTrue();
            
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);
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
            
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }
        
        [Test]
        public void TryWithLock_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Action<List>((v) =>
            {
                throw new CustomException();
            })));

            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
                return 1;
            }, out var result).ShouldBeTrue();

            result.ShouldBe(1);
            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);
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
        }
        
        [Test]
        public void TryWithLockFunc_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Func<List, int>((v) =>
            {
                throw new CustomException();
            }), out var result));

            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void Lock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            using (var guard = subject.Lock())
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                guard.Value.ShouldBe(value);
            }

            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);

            // act and assert
            using (var guard = subject.TryLock())
            {
                rwLock.IsWriteLockHeld.ShouldBeTrue();
                guard.Value.ShouldBe(value);
            }

            rwLock.IsWriteLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new List();
            var subject = new ReaderWriterSynchronized<List, List>(rwLock, value, value);
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
                rwLock.IsWriteLockHeld.ShouldBeFalse();
                guard.ShouldBeNull();
            }
        }
    }
}
