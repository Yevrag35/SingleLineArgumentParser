using MG.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    /// <summary>
    /// Indicates that the decorated property or field is an argument that should be mapped when parsed by
    /// a <see cref="Parser"/> instance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// The name of the argument that should be parsed.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Additional names for the <see cref="Parser"/> to match this argument by.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentAttribute"/> with the specified argument name and optional
        /// aliases.
        /// </summary>
        /// <param name="name">The name of the argument to match.</param>
        /// <param name="aliases">Additional names for the argument.</param>
        public ArgumentAttribute(string name, params string[] aliases)
        {
            this.Name = name;
            this.Aliases = aliases ?? Array.Empty<string>();
        }
    }
}
