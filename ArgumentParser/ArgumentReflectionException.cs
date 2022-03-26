using System;
using System.Globalization;
using System.Reflection;

namespace ArgumentParser
{
    /// <summary>
    /// Thrown when a reflection <see cref="Exception"/> occurs when applying values to 
    /// <see cref="PropertyInfo"/> and/or <see cref="FieldInfo"/> instances.
    /// </summary>
    public class ArgumentReflectionException : TargetException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentReflectionException"/> with the specified message and inner exception that caused
        /// the exception. 
        /// </summary>
        /// <param name="innerException">The underlying <see cref="Exception"/> that caused the <see cref="ArgumentReflectionException"/>.</param>
        /// <param name="message">The message format to be displayed with the <see cref="ArgumentReflectionException"/>.</param>
        /// <param name="arguments">Any arguments to be passed into the <paramref name="message"/> format.</param>
        public ArgumentReflectionException(Exception innerException, string message, params object[] arguments)
            : base(string.Format(CultureInfo.CurrentCulture, message, arguments), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentReflectionException"/> with the inner exception that caused
        /// the exception.
        /// </summary>
        /// <param name="innerException">The underlying <see cref="Exception"/> that caused the <see cref="ArgumentReflectionException"/>.</param>
        public ArgumentReflectionException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}
