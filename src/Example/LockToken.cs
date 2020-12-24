using InSync;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class LockToken
    {
        private class WriteToken
        {
            private WriteToken() { }

            /// <summary>
            /// This should be created once per object only.
            /// </summary>
            /// <returns></returns>
            public static WriteToken CreatePerObjectOnly() => new WriteToken();
        }

        private readonly AsyncSynchronized<WriteToken> writeToken = AsyncSynchronized.Create(WriteToken.CreatePerObjectOnly());

        public async Task Foo()
        {
            using (var w = await writeToken.LockAsync())
            {
                ReusableMethod(w.Value);
            }
        }

        public async Task Bar()
        {
            using (var w = await writeToken.LockAsync())
            {
                ReusableMethod(w.Value);
            }
        }

        private void ReusableMethod(WriteToken _)
        {
            // some complicated code
        }
    }
}
