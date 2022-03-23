using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    public sealed class ArgumentDetails
    {
        public string Name { get; }
        public string[] Aliases { get; }

        internal ArgumentDetails(ArgumentAttribute att)
        {
            this.Name = att.Name;
            this.Aliases = att.Aliases
                ?? Array.Empty<string>();
        }
    }
}
