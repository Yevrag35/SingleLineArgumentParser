using MG.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private const string REGEX_FORMAT = @"(?<!\S){0}(?'Command'\w+)(?:\s+|\=)(?:(?:\""(?'Value'.+?)\"")|(?'Value'[\w\,\+\?\-\=\*\&\%\$\#\@\!\(\)\^\;\:\[\]\\\|]+)|(?:\'(?'Value'.+?)\')|(?'Value'))";

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
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        public Parser(ParserOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
        /// <exception cref="ArgumentParsingException">
        ///     A duplicate argument was parsed and the specified <see cref="ParserOptions.AllowDuplicates"/>
        ///     is <see langword="false"/>.
        /// </exception>
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
        /// <exception cref="ArgumentParsingException">
        ///     A duplicate argument was parsed and the specified <see cref="ParserOptions.AllowDuplicates"/>
        ///     is <see langword="false"/>.
        /// </exception>
        /// <exception cref="ArgumentReflectionException">
        ///     One or more of the matching arguments could be not set to a property/field on type <typeparamref name="T"/>.
        /// </exception>
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
        /// <exception cref="ArgumentParsingException">
        ///     A duplicate argument was parsed and the specified <see cref="ParserOptions.AllowDuplicates"/>
        ///     is <see langword="false"/>.
        /// </exception>
        public IDictionary<string, object> Parse(string rawArguments, out string remainingText)
        {
            ArgumentDictionary dictionary = ParseCheck(rawArguments, _options);

            return dictionary.ToDictionary(out remainingText);
        }

        #endregion

        /// <exception cref="ArgumentReflectionException">
        ///     <paramref name="memberInfo"/> threw an exception when attempting to set its value to <paramref name="value"/>.
        /// </exception>
        private static void ApplyMemberValue<T>(T obj, MemberInfo memberInfo, object? value)
        {
            if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo pi)
            {
                try
                {
                    ApplyPropertyValue(obj, pi, value);
                }
                catch (Exception propEx)
                {
                    throw new ArgumentReflectionException(propEx);
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
                    throw new ArgumentReflectionException(fieldEx);
                }
            }
            else
            {
                var invalid = new InvalidOperationException(
                    message: $"'{nameof(memberInfo)}' is not a property or field.");

                throw new ArgumentReflectionException(invalid);
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
        private static void ApplyFieldValue(object? obj, FieldInfo fi, object? value)
        {
            if (null != obj)
            {
                fi.SetValue(obj, value);
            }
        }

        /// <exception cref="ArgumentReflectionException"></exception>
        /// <exception cref="TargetException">
        ///     In theportal class library catch <see cref="Exception"/> instead. <paramref name="obj"/> is 
        ///     <see langword="null"/> and the method is not <see langword="static"/>. -or- The method is not declared or inherited by 
        ///     the class of <paramref name="obj"/>. -or- A <see langword="static"/> constructor is invoked, and 
        ///     <paramref name="obj"/> is neither <see langword="null"/> nor an instance of the class that declared the constructor.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The elements of the parameters array do not match the signature of the method
        ///     or constructor reflected by this instance.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     The current instance is a System.Reflection.Emit.MethodBuilder.
        /// </exception>
        private static void ApplyPropertyValue<T>(T obj, PropertyInfo pi, object? value)
        {
            MethodInfo? setAcc = pi.GetSetMethod();
            if (setAcc is null)
            {
                setAcc = pi.GetSetMethod(true);
            }

            if (setAcc is null)
            {
                throw new ArgumentReflectionException(
                    innerException: new MissingMethodException(
                        className: pi.DeclaringType.FullName, 
                        methodName: $"{pi.Name}_set"),
                    message: $"Property '{pi.Name}' on type '{typeof(T).Name}' has no available set accessor.");
            }

            setAcc.Invoke(obj, new object?[] { value });
        }
        private static object? ConvertEnumerable(MemberInfo? memInfo, Type toType, object? value, object? existingValue)
        {
            if (!(value is List<object> list) || list.Count <= 0)
            { 
                return existingValue;
            }

            if (toType.IsArray)
            {
                return CreateArray(toType, list, existingValue);
            }

            if (existingValue is null)
            {
                existingValue = Activator.CreateInstance(toType);
            }

            Type[] genericArguments = toType.GetGenericArguments();
            Func<object?, object?> func = genericArguments.Length switch
            {
                0 => (incoming) => incoming,
                _ => (incoming) => ConvertObject(memInfo, genericArguments[0], incoming, null),
            };
            MethodInfo addMethod = FindAddMethod(toType);

            list.ForEach(o =>
            {
                object? converted = func(o);
                ExecuteAddMethod(addMethod, existingValue, converted);
            });

            return existingValue;
        }
        private static object? ConvertObject(MemberInfo? memInfo, Type toType, object? value, object? existingValue)
        {
            Type enumerableType = typeof(IEnumerable);
            if (toType.IsArray || (!toType.IsValueType && !toType.Equals(typeof(string)) && toType.GetInterfaces().Any(x => enumerableType.IsAssignableFrom(x))))
            {
                return ConvertEnumerable(memInfo, toType, value, existingValue);
            }
            else
            {
                return Convert.ChangeType(value, toType);
            }
        }
        private static object? ConvertValue<T>(T obj, MemberInfo memberInfo, object? rawValue)
        {
            ArgumentAttribute? argAtt = memberInfo
                .GetCustomAttributes<ArgumentAttribute>()
                .FirstOrDefault();

            if (argAtt is null)
            {
                return null;
            }

            object? existingValue = GetExistingValue(obj, memberInfo, out Type valueType);

            if (TryGetConverter(memberInfo, out ArgumentConverter? converter) && converter.CanConvert(valueType))
            {
                return converter.Convert(argAtt, rawValue, existingValue);
            }
            else
            {
                return ConvertObject(memberInfo, valueType, rawValue, existingValue);
            }
        }
        private static object CreateArray(Type arrayType, List<object> list, object? existingValue)
        {
            Type elementType = arrayType.GetElementType();
            if (!(existingValue is Array existingArray))
            {
                existingArray = Array.CreateInstance(elementType, 0);
            }

            Array arr = Array.CreateInstance(elementType, list.Count + existingArray.Length);

            if (existingArray.Length > 0)
            {
                existingArray.CopyTo(arr, 0);
            }

            for (int i = existingArray.Length; i < list.Count + existingArray.Length; i++)
            {
                object? converted = ConvertObject(null, elementType, list[i - existingArray.Length], null);
                arr.SetValue(converted, i);
            }

            return arr;
        }
        private static void ExecuteAddMethod(MethodInfo addMethod, object? collection, object? toBeAdded)
        {
            if (null == collection || null == toBeAdded)
            {
                return;
            }

            _ = addMethod.Invoke(collection, new object[] { toBeAdded });
        }
        private static MethodInfo FindAddMethod(Type enumerableType)
        {
            return enumerableType.GetMethod(
                name: "Add", 
                bindingAttr: BindingFlags.Instance | BindingFlags.Public);
        }
        private static object? GetExistingValue(object? fromObj, MemberInfo memberInfo, out Type memberValueType)
        {
            if (null == fromObj)
            {
                memberValueType = typeof(object);
                return null;
            }

            memberValueType = typeof(string);
            object? o;

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
            Type argAttType = typeof(ArgumentAttribute);

            return typeof(T)
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(mem =>
                    {
                        return mem
                            .CustomAttributes
                                .Any(att => 
                                    argAttType.IsAssignableFrom(att.AttributeType));
                    });
        }
        /// <exception cref="ArgumentParsingException">
        ///     A duplicate argument was parsed and the specified <see cref="ParserOptions.AllowDuplicates"/>
        ///     is <see langword="false"/>.
        /// </exception>
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
            RegexOptions rgOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

            if (options.CaseSensitive)
            {
                rgOptions |= RegexOptions.IgnoreCase;
            }

            MatchCollection collection = Regex.Matches(rawArguments, regex, rgOptions);
            ArgumentDictionary dict = new ArgumentDictionary(
                capacity: collection.Count, 
                comparer: options.GetComparer());

            for (int i = 0; i < collection.Count; i++)
            {
                Match match = collection[i];
                if (!match.Success || MatchHasGroupsButNoSuccess(match))
                {
                    continue;
                }

                Group commandGroup = match.Groups["Command"];
                if (commandGroup is null || !commandGroup.Success)
                {
                    continue;
                }    

                rawArguments = rawArguments.Replace(
                    oldValue: match.Groups[0].Value, 
                    newValue: string.Empty)
                    .Trim();

                string key = commandGroup.Value;

                if (dict.ContainsKey(key) && !(dict[key] is List<object>) && options.AllowDuplicates)
                {
                    object existingValue = dict[key];
                    if (dict.Remove(key))
                    {
                        if (!dict.Add(key, new List<object>(2) { existingValue }, out Exception caughtException))
                            throw new ArgumentParsingException(caughtException);
                    }
                }

                object value;
                Group valueGroup = match.Groups["Value"];
                if (valueGroup is null || !valueGroup.Success || string.IsNullOrWhiteSpace(valueGroup.Value))
                {
                    value = true;   // Then we'll treat this as a switch.
                }
                else
                {
                    value = valueGroup.Value.Trim();
                }

                if (!dict.Add(key, value, out Exception exception))
                {
                    throw new ArgumentParsingException(
                        innerException: exception, 
                        message: "Unable to add key '{0}' and value '{1}' to the ArgumentDictionary.",
                        key, value);
                }
            }

            dict.RemainingText = rawArguments;

            return dict;
        }

        private static bool MatchHasGroupsButNoSuccess(Match match)
        {
            return match.Groups.Count > 0 && !match.Groups[0].Success;
        }

        private static T Reflect<T>(T obj, ArgumentDictionary captured)
        {
            IEnumerable<MemberInfo> memberInfos = GetMembers<T>();

            foreach (MemberInfo memInfo in memberInfos)
            {
                if (captured.TryGetFromMember(memInfo, out object value))
                {
                    object? convertedValue = ConvertValue(obj, memInfo, value);
                    ApplyMemberValue(obj, memInfo, convertedValue);
                }
            }

            return obj;
        }
        private static bool TryGetConverter(MemberInfo propertyInfo, [NotNullWhen(true)] out ArgumentConverter? converter)
        {
            converter = null;
            ArgumentConverterAttribute? att = propertyInfo
                .GetCustomAttributes<ArgumentConverterAttribute>()
                    .FirstOrDefault();

            if (att is null)
            {
                return false;
            }

            converter = (ArgumentConverter)Activator.CreateInstance(att.ConverterType);
            return !(converter is null);
        }
    }
}
