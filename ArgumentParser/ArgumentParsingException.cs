using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ArgumentParser
{
    /// <summary>
    /// An exception class specifying that an unhandled parsing <see cref="Exception"/> occurred.
    /// </summary>
    public class ArgumentParsingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentParsingException"/> with the specified message and inner exception that caused
        /// the exception.
        /// </summary>
        /// <param name="innerException">The underlying <see cref="Exception"/> that caused the <see cref="ArgumentParsingException"/>.</param>
        /// <param name="message">The message format to be displayed with the <see cref="ArgumentParsingException"/>.</param>
        /// <param name="arguments">Any arguments to be passed into the <paramref name="message"/> format.</param>
        public ArgumentParsingException(Exception innerException, string message, params object[] arguments)
            : base(string.Format(CultureInfo.CurrentCulture, message, arguments), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentParsingException"/> with the specified inner exception that caused
        /// the exception.
        /// </summary>
        /// <param name="innerException">The underlying <see cref="Exception"/> that caused the <see cref="ArgumentParsingException"/>.</param>
        public ArgumentParsingException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}
