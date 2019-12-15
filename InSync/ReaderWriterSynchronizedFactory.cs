using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSync
{
    public static class ReaderWriterSynchronizedFactory
    {
        public static ReaderWriterSynchronized<TWrite, TRead> Create<TWrite, TRead>(ReaderWriterLockSlim readerWriterLockSlim, TWrite writeValue, TRead readValue)
            where TWrite : class
            where TRead : class
        {
            return new ReaderWriterSynchronized<TWrite, TRead>(readerWriterLockSlim, writeValue, readValue);
        }

        public static ReaderWriterSynchronized<TWrite, TWrite> Create<TWrite>(ReaderWriterLockSlim readerWriterLockSlim, TWrite writeValue)
            where TWrite : class
        {
            return new ReaderWriterSynchronized<TWrite, TWrite>(readerWriterLockSlim, writeValue, writeValue);
        }
    }
}
