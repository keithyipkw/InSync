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
            using (var guard = MultiSync.All(new[] { lock1, lock2 }))
            {
                var list1 = guard.Value[0];
                var list2 = guard.Value[1];
                list1.AddRange(list2);
            }
        }
    }
}
