using MG.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ArgumentAttribute : Attribute, IValueAttribute
    {
        public string Name { get; }
        public string[] Aliases { get; }

        object IValueAttribute.Value => new ArgumentDetails(this);

        public ArgumentAttribute(string name, params string[] aliases)
        {
            this.Name = name;
            this.Aliases = aliases ?? Array.Empty<string>();
        }

        public T GetAs<T>()
        {
            if (!typeof(T).Equals(typeof(ArgumentDetails)))
                throw new NotImplementedException();

            else
                return (T)(object)new ArgumentDetails(this);
        }

        public bool ValueIsString()
        {
            return false;
        }
    }
}
