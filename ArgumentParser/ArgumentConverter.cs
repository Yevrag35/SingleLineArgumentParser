using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    /// <summary>
    /// Converts an argument's value.
    /// </summary>
    public abstract class ArgumentConverter
    {
        /// <summary>
        /// Determines whether this <see cref="ArgumentConverter"/> can the specified object <see cref="Type"/>.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>
        ///     <see langword="true"/> if this instance can convert the specified object <see cref="Type"/>;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool CanConvert(Type objectType);
        /// <summary>
        /// Converts the parsed value from the command line.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <returns>
        ///     The converted object.
        /// </returns>
        public abstract object Convert(ArgumentAttribute attribute, object rawValue, object existingValue);
    }

    /// <summary>
    /// Converts an argument's value to an object of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The <see cref="Type"/> to convert the value to.</typeparam>
    public abstract class ArgumentConverter<TValue> : ArgumentConverter
    {
        /// <summary>
        /// Converts the parsed value from the command line to an object of type <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <param name="hasExistingValue"><paramref name="existingValue"/> has a value.</param>
        /// <returns>
        ///     The converted object of type <typeparamref name="TValue"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="existingValue"/> is of the wrong <see cref="Type"/>.</exception>
        public abstract TValue Convert(ArgumentAttribute attribute, object rawValue, TValue existingValue, bool hasExistingValue);
        /// <summary>
        /// Converts the parsed value from the command line.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <returns>
        ///     The converted object.
        /// </returns>
        public sealed override object Convert(ArgumentAttribute attribute, object rawValue, object existingValue)
        {
            bool existingIsNull = existingValue is null;
            if (!(existingValue is null || existingValue is TValue))
                throw new ArgumentException(string.Format("Converter cannot process the existing value.  '{0}' is required.",
                    typeof(TValue)));

            return this.Convert(attribute, rawValue, existingIsNull ? default : (TValue)existingValue, !existingIsNull);
        }
        /// <summary>
        /// Determines whether this <see cref="ArgumentConverter"/> can the specified object <see cref="Type"/>.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>
        ///     <see langword="true"/> if this instance can convert the specified object <see cref="Type"/>;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public sealed override bool CanConvert(Type objectType)
        {
            return typeof(TValue).IsAssignableFrom(objectType);
        }
    }
    /// <summary>
    /// Converts the parsed value from the command line to an object of type <typeparamref name="TValue"/> that had been 
    /// matched from the specific attribute of type <typeparamref name="TAttribute"/>.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Type"/> of <see cref="ArgumentAttribute"/> that the argument matched.</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> to convert the value to.</typeparam>
    public abstract class ArgumentConverter<TAttribute, TValue> : ArgumentConverter<TValue>
        where TAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Converts the parsed value from the command line to an object of type <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <param name="hasExistingValue"><paramref name="existingValue"/> has a value.</param>
        /// <param name="attributeType">The <see cref="Type"/> of attribute that the argument matched.</param>
        /// <returns>
        ///     The converted object of type <typeparamref name="TValue"/>.
        /// </returns>
        public abstract TValue Convert(TAttribute attribute, object rawValue, TValue existingValue, bool hasExistingValue, Type attributeType);

        /// <summary>
        /// Converts the parsed value from the command line to an object of type <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <param name="hasExistingValue"><paramref name="existingValue"/> has a value.</param>
        /// <returns>
        ///     The converted object of type <typeparamref name="TValue"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
        public sealed override TValue Convert(ArgumentAttribute attribute, object rawValue, TValue existingValue, bool hasExistingValue)
        {
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            Type attType = attribute.GetType();
            return this.Convert((TAttribute)attribute, rawValue, existingValue, hasExistingValue, attType);
        }
    }
}