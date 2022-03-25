using MG.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArgumentParser
{
    public sealed class Parser
    {
        private const string REGEX_FORMAT = @"\{0}(\w+)(?:$|(?:(?:\s|\=)(?:((?:\S+)|\"".+\""))))";
        private const string DOUBLE_QUOTE = "\"";
        //private const string SINGLE_QUOTE = "'";
        private readonly ParserOptions _options;

        public Parser()
            : this(new ParserOptions())
        {
        }
        public Parser(ParserOptions options)
        {
            _options = options;
        }

        public T MapArguments<T>(T obj, string rawArguments, out string remainingText)
            where T : class
        {
            remainingText = rawArguments;
            if (string.IsNullOrWhiteSpace(rawArguments))
                return obj;

            ArgumentDictionary arguments = ParseCheck(rawArguments, _options);
            remainingText = arguments.RemainingText;
            if (arguments.Count <= 0)
            {
                return obj;
            }

            return Reflect(obj, arguments);
        }
        public T MapArguments<T>(string rawArguments, out string remainingText)
            where T : class, new()
        {
            return this.MapArguments(new T(), rawArguments, out remainingText);
        }

        private static void ApplyMemberValue<T>(T obj, MemberInfo memberInfo, object value)
        {
            if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo pi)
            {
                ApplyPropertyValue(obj, pi, value);
            }
            else if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fi)
            {
                ApplyFieldValue(obj, fi, value);
            }
        }
        private static void ApplyPropertyValue<T>(T obj, PropertyInfo pi, object value)
        {
            MethodInfo setAcc = pi.GetSetMethod();
            if (setAcc is null)
                setAcc = pi.GetSetMethod(true);

            if (setAcc is null)
                throw new InvalidOperationException($"Property '{pi.Name}' on type '{typeof(T).Name}' has no available set accessor.");

            setAcc.Invoke(obj, new object[1] { value });
        }
        private static void ApplyFieldValue<T>(T obj, FieldInfo fi, object value)
        {
            fi.SetValue(obj, value);
        }
        private static ArgumentDictionary ParseCheck(string rawArguments, ParserOptions options)
        {
            if (string.IsNullOrWhiteSpace(rawArguments))
            {
                ArgumentDictionary d = new ArgumentDictionary(options.GetComparer());
                d.RemainingText = rawArguments;

                return d;
            }

            string regex = string.Format(REGEX_FORMAT, options.Prefix);
            RegexOptions rgOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            if (options.CaseSensitive)
                rgOptions |= RegexOptions.IgnoreCase;

            MatchCollection collection = Regex.Matches(rawArguments, regex, rgOptions);
            ArgumentDictionary dict = new ArgumentDictionary(collection.Count, options.GetComparer());

            for (int i = 0; i < collection.Count; i++)
            {
                Match match = collection[i];
                rawArguments = rawArguments.Replace(match.Groups[0].Value, string.Empty);

                string key = match.Groups[1].Value.Trim();
                object value = match.Groups.Count > 2 && match.Groups[2].Success
                    ? (object)TrimQuotes(match.Groups[2].Value)
                    : true;

                dict.Add(key, value);
            }

            dict.RemainingText = rawArguments.Trim();

            return dict;
        }
        private static T Reflect<T>(T obj, ArgumentDictionary captured)
        {
            IEnumerable<PropertyInfo> props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property =>
                    property
                        .GetCustomAttributes<ArgumentAttribute>()
                            .Any()
                );

            foreach (PropertyInfo pi in props)
            {
                if (captured.TryGetFromMember(pi, out object value))
                {
                    object convertedValue = ConvertValue(obj, pi, value);
                    ApplyPropertyValue(obj, pi, convertedValue);
                }
            }

            return obj;
        }
        private static object ConvertObject(PropertyInfo propertyInfo, object value)
        {
            return Convert.ChangeType(value, propertyInfo.PropertyType);
        }
        private static object ConvertValue<T>(T obj, PropertyInfo propertyInfo, object rawValue)
        {
            if (propertyInfo.PropertyType.Equals(typeof(bool)) && rawValue is bool)
                return rawValue;

            ArgumentAttribute argAtt = propertyInfo.GetCustomAttributes<ArgumentAttribute>()
                .FirstOrDefault();

            if (argAtt is null)
                return null;
            
            if (TryGetConverter(propertyInfo, out ArgumentConverter converter) && converter.CanConvert(propertyInfo.PropertyType))
            {
                object existingValue = propertyInfo.GetValue(obj);
                return converter.Convert(argAtt, rawValue, existingValue);
            }
            else
            {
                return ConvertObject(propertyInfo, rawValue);
            }
        }

        private static bool TryGetConverter(PropertyInfo propertyInfo, out ArgumentConverter converter)
        {
            converter = null;
            ArgumentConverterAttribute att = propertyInfo
                .GetCustomAttributes<ArgumentConverterAttribute>()
                    .FirstOrDefault();
            if (att is null)
                return false;

            converter = (ArgumentConverter)Activator.CreateInstance(att.ConverterType);
            return !(converter is null);
        }
        private static string TrimQuotes(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            if (str.StartsWith(DOUBLE_QUOTE) && str.EndsWith(DOUBLE_QUOTE))
                str = str.TrimStart((char)34).TrimEnd((char)34);

            return str;
        }
    }
}
