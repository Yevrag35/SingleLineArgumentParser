using MG.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArgumentParser
{
    /// <summary>
    /// A class used for parsing arguments from a single <see cref="string"/> command line intepretation.
    /// </summary>
    public sealed class Parser
    {
        private const string REGEX_FORMAT = @"{0}(\S+?)(?:$|(?:(?:\s|\=)(?:((?:(?:(?:\w|\-|\)|\(|\:)+)|(?:(?:\').+(?:\'))|(?:(?:(?:\w|\-|\)|\(|\:)+)|(?:(?:\"").+(?:\""))))))))";
        private const string DOUBLE_QUOTE = "\"";
        private const string SINGLE_QUOTE = "'";
        private readonly ParserOptions _options;

        /// <summary>
        /// The default constructor using a default <see cref="ParserOptions"/> instance.
        /// </summary>
        public Parser()
            : this(new ParserOptions())
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="Parser"/> using the specified <see cref="ParserOptions"/>.
        /// </summary>
        /// <param name="options">The options that the <see cref="Parser"/> will use.</param>
        public Parser(ParserOptions options)
        {
            _options = options;
        }

        #region PUBLIC METHODS
        /// <summary>
        /// Maps arguments to properties/fields of a previously constructed class instance.
        /// </summary>
        /// <typeparam name="T">The type of object whose properties/fields will be mapped.</typeparam>
        /// <param name="obj">The object whose properties/fields will be mapped.</param>
        /// <param name="rawArguments">The single <see cref="string"/> command line where the arguments will be parsed from.</param>
        /// <param name="remainingText">The <see cref="string"/> that remains after all arguments and values have been parsed out.</param>
        /// <returns>
        ///     The previously constructed object of type <typeparamref name="T"/> with any matching properties/fields having been set.
        /// </returns>
        /// <exception cref="ArgumentParsingException"></exception>
        /// <exception cref="ArgumentReflectionException"></exception>
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
        /// <summary>
        /// Constructs a new instance of <typeparamref name="T"/> and maps arguments to its properties/fields.
        /// </summary>
        /// <typeparam name="T">The type of object that will be constructed and whose properties/fields will be mapped.</typeparam>
        /// <param name="rawArguments">The single <see cref="string"/> command line where the arguments will be parsed from.</param>
        /// <param name="remainingText">The <see cref="string"/> that remains after all arguments and values have been parsed out.</param>
        /// <returns>
        ///     A new instance of <typeparamref name="T"/> with any matching properties/fields having been set.
        /// </returns>
        /// <exception cref="ArgumentParsingException"></exception>
        /// <exception cref="ArgumentReflectionException"></exception>
        public T MapArguments<T>(string rawArguments, out string remainingText)
            where T : class, new()
        {
            return this.MapArguments(new T(), rawArguments, out remainingText);
        }

        /// <summary>
        /// Parses the given argument <see cref="string"/> or command line input and returns a dictionary of any argument/value pairs
        /// found based on the <see cref="ParserOptions"/> given to the <see cref="Parser"/>.
        /// </summary>
        /// <param name="rawArguments">The single <see cref="string"/> command line where the arguments will be parsed from.</param>
        /// <param name="remainingText">
        ///     The <see cref="string"/> that remains after all arguments and values have been parsed out.
        /// </param>
        /// <returns>
        ///     An <see cref="IDictionary{TKey, TValue}"/> with <see cref="string"/> keys and values.  If an argument was matched but had 
        ///     no corresponding value, the value will be <see cref="string.Empty"/>.
        ///     
        ///     The out parameter <paramref name="remainingText"/> will be the <see cref="string"/> value of any remaining text that did
        ///     parse as arguments.  If all text has been parsed into argument/value pairs, then <paramref name="remainingText"/> will
        ///     be <see cref="string.Empty"/>.
        /// </returns>
        /// <exception cref="ArgumentParsingException"/>
        public IDictionary<string, object> Parse(string rawArguments, out string remainingText)
        {
            ArgumentDictionary dictionary = ParseCheck(rawArguments, _options);

            return dictionary.ToDictionary(out remainingText);
        }

        #endregion

        /// <exception cref="ArgumentParsingException">
        ///     
        /// </exception>
        private static void ApplyMemberValue<T>(T obj, MemberInfo memberInfo, object value)
        {
            if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo pi)
            {
                try
                {
                    ApplyPropertyValue(obj, pi, value);
                }
                catch (Exception propEx)
                {
                    throw new ArgumentParsingException(propEx);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fi)
            {
                try
                {
                    ApplyFieldValue(obj, fi, value);
                }
                catch (Exception fieldEx)
                {
                    throw new ArgumentParsingException(fieldEx);
                }
            }
            else
            {
                var invalid = new InvalidOperationException($"'{nameof(memberInfo)}' is not a property or field.");
                throw new ArgumentParsingException(invalid);
            }
        }
        /// <exception cref="FieldAccessException">
        ///     In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        ///     or the portal class library,
        ///     catch the base class exception, <see cref="MemberAccessException"/>, instead. The caller
        ///     does not have permission to access this field.
        /// </exception>
        /// <exception cref="TargetException">
        ///     In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        ///     or the portal class library,
        ///     catch <see cref="Exception"/> instead. <paramref name="obj"/> is <see langword="null"/> and the field is an
        ///     instance field.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The field does not exist on <paramref name="obj"/>. -or- <paramref name="value"/> cannot be converted
        ///     and stored in the field.
        /// </exception>
        private static void ApplyFieldValue(object obj, FieldInfo fi, object value)
        {
            fi.SetValue(obj, value);
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
        private static object ConvertObject(MemberInfo memInfo, Type toType, object value)
        {
            return Convert.ChangeType(value, toType);
        }
        private static object ConvertValue<T>(T obj, MemberInfo memberInfo, object rawValue)
        {
            ArgumentAttribute argAtt = memberInfo.GetCustomAttributes<ArgumentAttribute>()
                .FirstOrDefault();

            if (argAtt is null)
                return null;

            object existingValue = GetExistingValue(obj, memberInfo, out Type valueType);

            if (TryGetConverter(memberInfo, out ArgumentConverter converter) && converter.CanConvert(valueType))
            {
                return converter.Convert(argAtt, rawValue, existingValue);
            }
            else
            {
                return ConvertObject(memberInfo, valueType, rawValue);
            }
        }
        private static object GetExistingValue(object fromObj, MemberInfo memberInfo, out Type memberValueType)
        {
            memberValueType = typeof(string);
            object o;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    PropertyInfo pi = (PropertyInfo)memberInfo;
                    o = pi.GetValue(fromObj);
                    memberValueType = pi.PropertyType;
                    break;

                case MemberTypes.Field:
                    FieldInfo fi = (FieldInfo)memberInfo;
                    o = fi.GetValue(fromObj);
                    memberValueType = fi.FieldType;
                    break;

                default:
                    o = null;
                    break;
            }

            return o;
        }
        private static IEnumerable<MemberInfo> GetMembers<T>()
        {
            return typeof(T)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(mem =>
                    {
                        return mem
                            .GetCustomAttributes<ArgumentAttribute>()
                                .Any();
                    });
        }
        /// <exception cref="ArgumentParsingException"></exception>
        private static ArgumentDictionary ParseCheck(string rawArguments, ParserOptions options)
        {
            if (string.IsNullOrWhiteSpace(rawArguments))
            {
                ArgumentDictionary d = new ArgumentDictionary(options.GetComparer())
                {
                    RemainingText = rawArguments ?? string.Empty
                };

                return d;
            }

            string regex = string.Format(REGEX_FORMAT, Regex.Escape(options.Prefix));
            RegexOptions rgOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            if (options.CaseSensitive)
                rgOptions |= RegexOptions.IgnoreCase;

            MatchCollection collection = Regex.Matches(rawArguments, regex, rgOptions);
            ArgumentDictionary dict = new ArgumentDictionary(collection.Count, options.GetComparer());

            for (int i = 0; i < collection.Count; i++)
            {
                Match match = collection[i];
                rawArguments = rawArguments.Replace(match.Groups[0].Value, string.Empty).Trim();

                string key = match.Groups[1].Value.Trim();
                if (dict.ContainsKey(key) && options.AllowDuplicates)
                {
                    object existingValue = dict[key];
                    if (dict.Remove(key))
                    {
                        if (!dict.Add(key, new List<object>(2) { existingValue }, out Exception caughtException))
                            throw new ArgumentParsingException(caughtException);
                    }
                }

                object value = match.Groups.Count > 2 && match.Groups[2].Success
                    ? (object)TrimQuotes(match.Groups[2].Value.Trim())
                    : true;

                if (!dict.Add(key, value, out Exception exception))
                    throw new ArgumentParsingException(exception, "Unable to add key '{0}' and value '{1}' to the ArgumentDictionary.", key, value);
            }

            dict.RemainingText = rawArguments.Trim();

            return dict;
        }
        private static T Reflect<T>(T obj, ArgumentDictionary captured)
        {
            IEnumerable<MemberInfo> memberInfos = GetMembers<T>();

            foreach (MemberInfo memInfo in memberInfos)
            {
                if (captured.TryGetFromMember(memInfo, out object value))
                {
                    object convertedValue = ConvertValue(obj, memInfo, value);
                    ApplyMemberValue(obj, memInfo, convertedValue);
                }
            }

            return obj;
        }
        private static bool TryGetConverter(MemberInfo propertyInfo, out ArgumentConverter converter)
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

            else if (str.StartsWith(SINGLE_QUOTE) && str.EndsWith(SINGLE_QUOTE))
                str = str.TrimStart((char)39).TrimEnd((char)39);

            return str;
        }
    }
}
