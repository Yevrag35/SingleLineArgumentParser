using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    /// <summary>
    /// A class that contains details about the <see cref="ArgumentAttribute"/>.
    /// </summary>
    public sealed class ArgumentDetails
    {
        /// <summary>
        /// The name of the argument that should be parsed.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Additional names for the <see cref="Parser"/> to match this argument by.
        /// </summary>
        public string[] Aliases { get; }

        internal ArgumentDetails(ArgumentAttribute att)
        {
            this.Name = att.Name;
            this.Aliases = att.Aliases
                ?? Array.Empty<string>();
        }
    }
}
