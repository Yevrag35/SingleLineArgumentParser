using MG.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ArgumentParser
{
    internal sealed class ArgumentDictionary
    {
        private readonly Dictionary<string, object> _dict;
        private static readonly AttributeValuator<ArgumentAttribute, ArgumentDetails> Valuator =
            new AttributeValuator<ArgumentAttribute, ArgumentDetails>(
                (att) => new ArgumentDetails(att)
            );

        public object this[string name]
        {
            get => _dict.TryGetValue(name, out object value)
                ? value
                : null;
        }

        public int Count => _dict.Count;
        public ICollection<string> Keys => _dict.Keys;
        public ICollection<object> Values => _dict.Values;

        public string RemainingText { get; internal set; }

        public ArgumentDictionary(IEqualityComparer<string> comparer)
            : this(0, comparer)
        { 
        }
        public ArgumentDictionary(int capacity, IEqualityComparer<string> comparer)
        {
            _dict = new Dictionary<string, object>(capacity, comparer);
        }

        public bool Add(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            bool result = false;
            if (!_dict.ContainsKey(key))
            {
                _dict.Add(key, value);
                result = true;
            }

            return result;
        }
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetFromMember(MemberInfo memberInfo, out object value)
        {
            value = null;

            ArgumentDetails details = Valuator.GetValue(memberInfo);
            if (!(details is null))
            {
                if (_dict.TryGetValue(details.Name, out object tempValue1))
                {
                    value = tempValue1;
                    return true;
                }

                for (int i = 0; i < details.Aliases.Length; i++)
                {
                    if (_dict.TryGetValue(details.Aliases[i], out object tempValue2))
                    {
                        value = tempValue2;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
