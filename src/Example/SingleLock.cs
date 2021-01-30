using InSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example
{
    class SingleLock
    {
        private readonly Synchronized<List<int>> list = Synchronized.Create(new List<int>());

        public void WithLock()
        {
            list.WithLock(list =>
            {
                list.Add(0);
                Console.WriteLine("locking");
            });
        }
        
        public int WithLockReturn()
        {
            return list.WithLock(list => list.FirstOrDefault());
        }
        
        public void TryWithLock()
        {
            if (list.TryWithLock(list => list.Add(0)))
            {
                Console.WriteLine("added");
            }
        }
        
        public int Guard()
        {
            using (var guard = list.Lock())
            {
                return guard.Value.FirstOrDefault();
            }
        }

        public int TryGuard()
        {
            using (var guard = list.TryLock())
            {
                if (guard != null)
                {
                    Console.WriteLine("locking");
                    return guard.Value.FirstOrDefault();
                }
                Console.WriteLine("not locked");
                return -1;
            }
        }

        private readonly Synchronized<ValueContainer<int>> container = Synchronized.Create(new ValueContainer<int>(0));

        public void Increase()
        {
            container.WithLock(c =>
            {
                ++c.Value;
            });
        }
    }
}
