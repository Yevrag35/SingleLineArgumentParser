using System;
using System.Collections.Generic;

namespace ArgumentParser
{
    public class ArgumentConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public ArgumentConverterAttribute(Type typeOfConverter)
        {
            this.ConverterType = typeOfConverter;
        }
    }
}