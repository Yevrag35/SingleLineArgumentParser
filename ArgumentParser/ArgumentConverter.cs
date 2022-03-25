using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    public abstract class ArgumentConverter
    {
        public abstract bool CanConvert(Type type);
        public abstract object Convert(ArgumentAttribute attribute, object rawValue, object existingValue);
    }

    public abstract class ArgumentConverter<TValue> : ArgumentConverter
    {
        public abstract TValue Convert(ArgumentAttribute attribute, object rawValue, TValue existingValue, bool hasExistingValue);
        public sealed override object Convert(ArgumentAttribute attribute, object rawValue, object existingValue)
        {
            bool existingIsNull = existingValue is null;
            if (!(existingValue is null || existingValue is TValue))
                throw new ArgumentException(string.Format("Converter cannot process the existing value.  '{0}' is required.",
                    typeof(TValue)));

            return this.Convert(attribute, rawValue, existingIsNull ? default : (TValue)existingValue, !existingIsNull);
        }
        public sealed override bool CanConvert(Type objectType)
        {
            return typeof(TValue).IsAssignableFrom(objectType);
        }
    }

    public abstract class ArgumentConverter<TAttribute, TValue> : ArgumentConverter<TValue>
        where TAttribute : ArgumentAttribute
    {
        public abstract TValue Convert(TAttribute attribute, object rawValue, TValue existingValue, bool hasExistingValue, Type attributeType);
        public sealed override TValue Convert(ArgumentAttribute attribute, object rawValue, TValue existingValue, bool hasExistingValue)
        {
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            Type attType = attribute.GetType();
            return this.Convert((TAttribute)attribute, rawValue, existingValue, hasExistingValue, attType);
        }
    }
}