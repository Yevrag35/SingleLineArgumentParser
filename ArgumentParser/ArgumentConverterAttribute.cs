using System;
using System.Collections.Generic;

namespace ArgumentParser
{
    /// <summary>
    /// Instructs the <see cref="Parser"/> to use the specified <see cref="ArgumentConverter"/> when parsing arguments.
    /// </summary>
    public class ArgumentConverterAttribute : Attribute
    {
        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="ArgumentConverter"/>.
        /// </summary>
        public Type ConverterType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentConverterAttribute"/>.
        /// </summary>
        /// <param name="typeOfConverter">Type of the <see cref="ArgumentConverter"/>.</param>
        public ArgumentConverterAttribute(Type typeOfConverter)
        {
            this.ConverterType = typeOfConverter;
        }
    }
}