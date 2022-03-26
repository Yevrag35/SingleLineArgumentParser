using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    /// <summary>
    /// A class specifying options for an instance of <see cref="Parser"/>.
    /// </summary>
    public class ParserOptions
    {
        private const string DEFAULT_PREFIX = "/";
        private string _prefix;

        /// <summary>
        /// Indicates that the <see cref="Parser"/> can map duplicate argument names.  Any duplicate arguments will have their
        /// values coalesced.
        /// </summary>
        public bool AllowDuplicates { get; set; }

        /// <summary>
        /// Indicates that argument names and aliases are matched using a case-sensitive string comparer.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        public bool CaseSensitive { get; set; }
        /// <summary>
        /// The prefix that proceeds all arguments in the <see cref="string"/>. If a <see langword="null"/> value is supplied to 
        /// the set accessor, the prefix will revert back to the default value.
        /// </summary>
        /// <remarks>Default: /</remarks>
        public string Prefix
        {
            get => _prefix ?? DEFAULT_PREFIX;
            set => _prefix = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ParserOptions"/>.
        /// </summary>
        public ParserOptions()
        {
            this.CaseSensitive = false;
            this.Prefix = DEFAULT_PREFIX;
        }

        /// <summary>
        /// Returns an <see cref="IEqualityComparer{T}"/> depending on the <see cref="CaseSensitive"/> property's value.
        /// </summary>
        /// <remarks>
        ///     When overriden in derived classes it can return custom string comparers, but should always return a <see cref="IEqualityComparer{T}"/>
        ///     that either adheres to the <see cref="CaseSensitive"/> option.
        /// </remarks>
        /// <returns>
        ///     Depending on the value of <see cref="CaseSensitive"/>, an <see cref="IEqualityComparer{T}"/> instance that either does
        ///     or does not adhere to <see cref="string"/> casing rules.
        /// </returns>
        public virtual IEqualityComparer<string> GetComparer()
        {
            return !this.CaseSensitive
                ? StringComparer.CurrentCultureIgnoreCase
                : StringComparer.CurrentCulture;
        }
    }
}
