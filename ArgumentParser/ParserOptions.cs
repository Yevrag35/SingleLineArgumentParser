using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    public class ParserOptions
    {
        public bool CaseSensitive { get; set; }
        public string Prefix { get; set; } = "/";

        protected internal virtual IEqualityComparer<string> GetComparer()
        {
            return !this.CaseSensitive
                ? StringComparer.CurrentCultureIgnoreCase
                : StringComparer.CurrentCulture;
        }
    }
}
