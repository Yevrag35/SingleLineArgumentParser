using MG.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArgumentParser
{
    public class Parser
    {
        private const string REGEX_FORMAT = @"{0}(\w+)(?:$|(?:(?:\s|\=)(?:((?:\w+)|\"".+\""))))";
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

        public T MapArguments<T>(string rawArguments, out string remainingText)
            where T : class, new()
        {
            remainingText = rawArguments;
            if (string.IsNullOrWhiteSpace(rawArguments))
                return new T();

            T obj = new T();

            ArgumentDictionary arguments = ParseCheck(rawArguments, _options);
            remainingText = arguments.RemainingText;
            if (arguments.Count <= 0)
            {
                return obj;
            }

            return Reflect(obj, arguments);
        }

        private static void ApplyValue<T>(T obj, PropertyInfo pi, object value)
        {
            MethodInfo setAcc = pi.GetSetMethod();
            if (setAcc is null)
                setAcc = pi.GetSetMethod(true);

            if (setAcc is null)
                throw new InvalidOperationException($"Property '{pi.Name}' on type '{typeof(T).Name}' has no available set accessor.");

            setAcc.Invoke(obj, new object[1] { value });
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
                    ? TryConvertString(match.Groups[2].Value)
                    : true;

                dict.Add(key, value);
            }

            dict.RemainingText = rawArguments.Trim();

            return dict;
        }
        private static T Reflect<T>(T obj, ArgumentDictionary captured)
        {
            Type argType = typeof(ArgumentAttribute);
            IEnumerable<PropertyInfo> props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x =>
                    x.CustomAttributes.Any(att =>
                        argType.IsAssignableFrom(att.AttributeType))
                );

            foreach (PropertyInfo pi in props)
            {
                if (captured.TryGetFromProperty(pi, out object value))
                {
                    ApplyValue(obj, pi, value);
                }
            }

            return obj;
        }
        private static object TryConvertString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            else if (str.StartsWith(DOUBLE_QUOTE) && str.EndsWith(DOUBLE_QUOTE))
                str = str.TrimStart((char)34).TrimEnd((char)34);

            if (long.TryParse(str, out long outLong))
                return outLong;

            else if (bool.TryParse(str, out bool outBool))
                return outBool;

            else if (Guid.TryParse(str, out Guid outGuid))
                return outGuid;

            else
                return str;
        }
    }
}
