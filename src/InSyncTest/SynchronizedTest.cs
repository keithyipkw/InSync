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
    public class SynchronizedTest : SynchronizedTestBase<Synchronized<object>, object, SynchronizationLockException>
    {
        private readonly object value = new object();
        private object padLock;

        [SetUp]
        public void Setup()
        {
            padLock = new object();
        }

        protected override void AssertLocked(object value)
        {
            Monitor.IsEntered(padLock).ShouldBeTrue();
            value.ShouldBeSameAs(this.value);
        }

        protected override void AssertNotLocked()
        {
            Monitor.IsEntered(padLock).ShouldBeFalse();
        }

        protected override Synchronized<object> Create(bool locked)
        {
            if (locked)
            {
                Monitor.TryEnter(padLock);
            }
            return new Synchronized<object>(padLock, value);
        }

        protected override Synchronized<object> CreateNotLockable()
        {
            var setupEvent = new AutoResetEvent(false);
            new Thread(() =>
            {
                Monitor.Enter(padLock);
                setupEvent.Set();
            }).Start();
            setupEvent.WaitOne();
            return new Synchronized<object>(padLock, value);
        }

        [Test]
        public void WithLockAction_Reentrant()
        {
            // setup
            var subject = Create(false);

            // act and assert
            subject.WithLock((v) =>
            {
                subject.WithLock((v2) =>
                {
                    AssertLocked(v2);
                });
            });

            AssertNotLocked();
        }

        [Test]
        public void Lock_Reentrant()
        {
            // setup
            var subject = Create(false);

            // act and assert
            using (var guard = subject.Lock())
            using (var guard2 = subject.Lock())
            {
                AssertLocked(guard2.Value);
            }

            AssertNotLocked();
        }
    }
}
