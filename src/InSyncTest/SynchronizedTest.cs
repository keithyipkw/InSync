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
    public class SynchronizedTest
    {
        [Test]
        public void BarelyLock_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            var result = subject.BarelyLock();

            result.ShouldBe(value);
            Monitor.IsEntered(padLock).ShouldBeTrue();
        }

        [Test]
        public void BarelyUnlock_Release()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);
            Monitor.Enter(padLock);

            // act and assert
            subject.BarelyUnlock();
            
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void BarelyTryLock_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            subject.BarelyTryLock(out Stack result).ShouldBeTrue();

            result.ShouldBe(value);
            Monitor.IsEntered(padLock).ShouldBeTrue();
        }
        
        [Test]
        public void BarelyTryLock_NotAcquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                Monitor.Enter(padLock);
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
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            subject.WithLock((v) =>
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                v.ShouldBe(value);
            });
            
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Exception_Release()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);
            
            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock((v) =>
            {
                throw new CustomException();
            }));

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void WithLockAction_Reentrant()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            subject.WithLock((v) =>
            {
                subject.WithLock((v2) =>
                {
                    Monitor.IsEntered(padLock).ShouldBeTrue();
                    v2.ShouldBe(value);
                });
            });

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void WithLockFunc_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            subject.WithLock((v) =>
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                v.ShouldBe(value);
                return 1;
            }).ShouldBe(1);
            
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void WithLockFunc_Exception_Release()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.WithLock(new Func<Stack, int>((v) =>
            {
                throw new CustomException();
            })));
            
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }
        
        [Test]
        public void TryWithLock_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                v.ShouldBe(value);
            }).ShouldBeTrue();
            
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryWithLock_NotAcquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                Monitor.Enter(padLock);
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            subject.TryWithLock((v) =>
            {
                Assert.Fail();
            }).ShouldBeFalse();
            
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryWithLock_Exception_Release()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Action<Stack>((v) =>
            {
                throw new CustomException();
            })));

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            subject.TryWithLock((v) =>
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                v.ShouldBe(value);
                return 1;
            }, out var result).ShouldBeTrue();

            result.ShouldBe(1);
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_NotAcquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                Monitor.Enter(padLock);
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
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryWithLockFunc_Exception_Release()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            Should.Throw<CustomException>(() => subject.TryWithLock(new Func<Stack, int>((v) =>
            {
                throw new CustomException();
            }), out var result));

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void Lock_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            using (var guard = subject.Lock())
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                guard.Value.ShouldBe(value);
            }

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void Lock_Reentrant()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            using (var guard = subject.Lock())
            using (var guard2 = subject.Lock())
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                guard2.Value.ShouldBe(value);
            }

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryLock_Acquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);

            // act and assert
            using (var guard = subject.TryLock())
            {
                Monitor.IsEntered(padLock).ShouldBeTrue();
                guard.Value.ShouldBe(value);
            }

            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        [Test]
        public void TryLock_NotAcquire()
        {
            // setup
            var padLock = new object();
            var value = new Stack();
            var subject = new Synchronized<Stack>(padLock, value);
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                Monitor.Enter(padLock);
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();

            // act and assert
            using (var guard = subject.TryLock())
            {
                Monitor.IsEntered(padLock).ShouldBeFalse();
                guard.ShouldBeNull();
            }
        }
    }
}
