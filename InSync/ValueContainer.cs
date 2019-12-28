using System;
using System.Collections.Generic;
using System.Text;

namespace InSync
{
    /// <summary>
    /// Provides a way to change the protected value or protect a <c>struct</c> in <seealso cref="Synchronized"/> and the variances.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueContainer<T>
    {
        /// <summary>
        /// The value to protect.
        /// </summary>
        public T Value;
    }
}
