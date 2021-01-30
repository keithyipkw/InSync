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
        /// Initializes a new instance of <seealso cref="ValueContainer{T}"/> that contains the value.
        /// </summary>
        /// <param name="value"></param>
        public ValueContainer(T value)
        {
            Value = value;
        }

        /// <summary>
        /// The value to protect.
        /// </summary>
        public T Value;
    }
}
