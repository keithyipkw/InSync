using InSync;
using System;
using System.Collections.Generic;
using System.Text;

namespace Example
{
    class MultipleLocks
    {
        private readonly Synchronized<List<int>> lock1 = Synchronized.Create(new List<int>());
        private readonly Synchronized<List<int>> lock2 = Synchronized.Create(new List<int>());

        public void UnorderedAcquisition()
        {
            using (var guard = MultiSync.All(lock1, lock2))
            {
                var list1 = guard.Value.Item1;
                var list2 = guard.Value.Item2;
                list1.AddRange(list2);
            }
        }
    }
}
