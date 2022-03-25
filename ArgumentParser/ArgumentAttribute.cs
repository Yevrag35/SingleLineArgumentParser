using MG.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : Attribute
    {
        public string Name { get; }
        public string[] Aliases { get; }

        public ArgumentAttribute(string name, params string[] aliases)
        {
            this.Name = name;
            this.Aliases = aliases ?? Array.Empty<string>();
        }
    }
}
