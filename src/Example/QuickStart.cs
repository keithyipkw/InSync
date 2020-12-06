using InSync;
using System;
using System.Collections.Generic;
using System.Text;

namespace Example
{
    class QuickStart
    {
        private class State
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private readonly Synchronized<State> state = Synchronized.Create(new State());

        public void Foo(int z)
        {
            state.WithLock(s =>
            {
                s.X += 1;
                s.Y += z;
            });
        }

        public void Bar(int z)
        {
            // alternative style
            using (var guard = state.Lock())
            {
                guard.Value.X -= 1;
                guard.Value.Y -= z;
            }
        }

        public (int X, int Y) Get()
        {
            // passes the return value
            return state.WithLock(s =>
            {
                return (s.X, s.Y);
            });
        }

        private void ReusableMethod(State state)
        {
            // useful for AsyncSynchronized<T> and ReaderWriterSynchronized<T> because they are non-reentrant
            state.X += 1;
        }
    }
}
