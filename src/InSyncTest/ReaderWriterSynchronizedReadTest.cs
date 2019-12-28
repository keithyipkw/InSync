using InSync;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSyncTest
{
    [TestFixture]
    public class ReaderWriterSynchronizedReadTest
    {
        [Test]
        public void BarelyLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            var result = subject.BarelyLock();

            result.ShouldBe(value);
            rwLock.IsReadLockHeld.ShouldBeTrue();
        }

        [Test]
        public void BarelyUnlock_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
            rwLock.EnterReadLock();

            // act and assert
            subject.BarelyUnlock();
            
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void BarelyTryLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            subject.BarelyTryLock(out Stack result).ShouldBeTrue();

            result.ShouldBe(value);
            rwLock.IsReadLockHeld.ShouldBeTrue();
        }

        [Test]
        public void BarelyTryLock_ConcurrentAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterReadLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.BarelyTryLock(out Stack result).ShouldBeTrue();

            result.ShouldBe(value);
            rwLock.IsReadLockHeld.ShouldBeTrue();
        }

        [Test]
        public void BarelyTryLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                rwLock.EnterWriteLock();
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.BarelyTryLock(out Stack result).ShouldBeFalse();

            result.ShouldBeNull();
        }

        [Test]
        public void WithLockAction_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            subject.WithLock((v) =>
            {
                rwLock.IsReadLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
            });
            
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
            
            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock((v) =>
            {
                throw new CustomException();
            }));

            rwLock.IsReadLockHeld.ShouldBeFalse();
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

        [Test]
        public void WithLockFunc_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            subject.WithLock((v) =>
            {
                rwLock.IsReadLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
                return 1;
            }).ShouldBe(1);
            
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void WithLockFunc_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock(new Func<Stack, int>((v) =>
            {
                throw new CustomException();
            })));
            
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }
        
        [Test]
        public void TryWithLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            subject.TryWithLock((v) =>
            {
                rwLock.IsReadLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
            }).ShouldBeTrue();
            
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
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
            
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLock_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Action<Stack>((v) =>
            {
                throw new CustomException();
            })));

            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            subject.TryWithLock((v) =>
            {
                rwLock.IsReadLockHeld.ShouldBeTrue();
                v.ShouldBe(value);
                return 1;
            }, out var result).ShouldBeTrue();

            result.ShouldBe(1);
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
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
            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Exception_Release()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Func<Stack, int>((v) =>
            {
                throw new CustomException();
            }), out var result));

            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void Lock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            using (var guard = subject.Lock())
            {
                rwLock.IsReadLockHeld.ShouldBeTrue();
                guard.Value.ShouldBe(value);
            }

            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryLock_Acquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;

            // act and assert
            using (var guard = subject.TryLock())
            {
                rwLock.IsReadLockHeld.ShouldBeTrue();
                guard.Value.ShouldBe(value);
            }

            rwLock.IsReadLockHeld.ShouldBeFalse();
        }

        [Test]
        public void TryLock_NotAcquire()
        {
            // setup
            var rwLock = new ReaderWriterLockSlim();
            var value = new Stack();
            var subject = new ReaderWriterSynchronized<Stack, Stack>(rwLock, value, value).Reader;
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
                rwLock.IsReadLockHeld.ShouldBeFalse();
                guard.ShouldBeNull();
            }
        }
    }
}
