using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InSync
{
    /// <summary>
    /// Provides static methods for creating <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/> without explicitly specifying the types of the protected reader and writer.
    /// </summary>
    public static class ReaderWriterSynchronizedFactory
    {
        /// <summary>
        /// Creates a <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/> with the specified <seealso cref="ReaderWriterLockSlim"/>, writer to protect and reader to protect.
        /// </summary>
        /// <typeparam name="TWrite">The type of the writer to protect.</typeparam>
        /// <typeparam name="TRead">The type of the reader to protect.</typeparam>
        /// <param name="readerWriterLockSlim">The <seealso cref="ReaderWriterLockSlim"/> for synchronization.</param>
        /// <param name="writer">The writer to protect.</param>
        /// <param name="reader">The reader to protect.</param>
        /// <returns>A new <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="readerWriterLockSlim"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public static ReaderWriterSynchronized<TWrite, TRead> Create<TWrite, TRead>(ReaderWriterLockSlim readerWriterLockSlim, TWrite writer, TRead reader)
            where TWrite : class
            where TRead : class
        {
            return new ReaderWriterSynchronized<TWrite, TRead>(readerWriterLockSlim, writer, reader);
        }

        /// <summary>
        /// Creates a <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/> the specified writer to protect and reader to protect. A new <seealso cref="ReaderWriterLockSlim"/> is created with default property values and used.
        /// </summary>
        /// <typeparam name="TWrite">The type of the writer to protect.</typeparam>
        /// <typeparam name="TRead">The type of the reader to protect.</typeparam>
        /// <param name="writer">The writer to protect.</param>
        /// <param name="reader">The reader to protect.</param>
        /// <returns>A new <seealso cref="ReaderWriterSynchronized{TWrite, TRead}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public static ReaderWriterSynchronized<TWrite, TRead> Create<TWrite, TRead>(TWrite writer, TRead reader)
            where TWrite : class
            where TRead : class
        {
            return new ReaderWriterSynchronized<TWrite, TRead>(new ReaderWriterLockSlim(), writer, reader);
        }

        /// <summary>
        /// Creates a <seealso cref="ReaderWriterSynchronized{T}"/> with the specified <seealso cref="ReaderWriterLockSlim"/> and object to protect.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="readerWriterLockSlim">The <seealso cref="ReaderWriterLockSlim"/> for synchronization.</param>
        /// <param name="value">The object to protect.</param>
        /// <returns>A new <seealso cref="ReaderWriterSynchronized{T}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="readerWriterLockSlim"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static ReaderWriterSynchronized<T> Create<T>(ReaderWriterLockSlim readerWriterLockSlim, T value)
            where T : class
        {
            return new ReaderWriterSynchronized<T>(readerWriterLockSlim, value);
        }

        /// <summary>
        /// Creates a <seealso cref="ReaderWriterSynchronized{T}"/> with the specified object to protect. A new <seealso cref="ReaderWriterLockSlim"/> is created with default property values and used.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="value">The object to protect.</param>
        /// <returns>A new <seealso cref="ReaderWriterSynchronized{T}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static ReaderWriterSynchronized<T> Create<T>(T value)
            where T : class
        {
            return new ReaderWriterSynchronized<T>(new ReaderWriterLockSlim(), value);
        }
    }
}
