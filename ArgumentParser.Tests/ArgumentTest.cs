using ArgumentParser.Tests.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ArgumentParser.Tests
{
    public class ArgumentTest
    {
        private const string Guid = "9957d0b7-3bf9-49ac-a32d-710b5d284a94";

        public static IEnumerable<object[]> CommandStrings(int take, int skip) => new object[][]
        {
            new object[]
            {
                "condex.exe /name \"whatup\" /time 10:00", "whatup", Convert.ToDateTime("10:00")
            },
            new object[]
            {
                "asdf.jpg, name whatup", "whatup", Convert.ToDateTime("10:00")
            }
        }.Skip(skip).Take(take);

        public static IEnumerable<object[]> DuplicateCommandString => new object[][]
        {
            new object[]
            {
                "--number 1 --number=2 --name 'hi there'", "number", "name", "hi there", new List<object>(2) { "1", "2" }
            }
        };

        [Fact]
        public void ConstructorTest()
        {
            var parser = new Parser();
            Assert.NotNull(parser);
        }

        [Fact]
        public void OptionsTest()
        {
            ParserOptions parserOptions = new();
            var options = parserOptions;
            Assert.NotNull(options.Prefix);

            Assert.False(options.CaseSensitive);

            ParserOptions options2 = new()
            {
                Prefix = null,
                CaseSensitive = true
            };

            Assert.NotNull(options2.Prefix);
            Assert.Equal(options.Prefix, options2.Prefix);

            Assert.True(options2.CaseSensitive);

            StringComparer ignoreCase = StringComparer.CurrentCultureIgnoreCase;
            StringComparer noIgnoreCase = StringComparer.CurrentCulture;

            Assert.Equal(ignoreCase, options.GetComparer());
            Assert.Equal(noIgnoreCase, options2.GetComparer());

            _ = new Parser(options);
        }

        [Theory]
        [MemberData(nameof(CommandStrings), 1, 0)]
        public void TestAutoMapping(string commandString, object expectedValue, DateTime time)
        {
            var parser = new Parser();
            var argument = parser.MapArguments<MockArgument>(commandString, out string remainingText);

            Assert.NotNull(argument);
            Assert.NotEqual(commandString, remainingText);

            Assert.Equal(expectedValue, argument.Name);
            Assert.Equal(time, argument.TheDate);
        }

        [Theory]
        [MemberData(nameof(CommandStrings), 1, 0)]
        public void TestAutoMappingPreLoad(string commandString, object expectedValue, DateTime time)
        {
            var parser = new Parser();
            var preLoad = new MockArgument();
            string guid = Guid;
            preLoad.Name = guid;   // Pre-define value to see if it overwrites.

            var argument = parser.MapArguments(preLoad, commandString, out string remainingText);

            Assert.NotNull(argument);
            Assert.NotEqual(commandString, remainingText);

            Assert.Equal(expectedValue, argument.Name);
            Assert.Equal(time, argument.TheDate);
            Assert.NotEqual(guid, argument.Name);
        }

        [Theory]
        [MemberData(nameof(CommandStrings), 1, 1)]
        public void TestAutoMappingFail(string commandString, object expectedValue, DateTime time)
        {
            var parser = new Parser();
            var argument = parser.MapArguments<MockArgument>(commandString, out string remainingText);

            Assert.NotNull(argument);
            Assert.Equal(commandString, remainingText);

            Assert.NotEqual(expectedValue, argument.Name);
            Assert.NotEqual(time, argument.TheDate);
        }

        [Theory]
        [MemberData(nameof(DuplicateCommandString))]
        public void TestDuplicateFail(string commandString, string param1Name, string param2Name, string param2Value, List<object> param1Value)
        {
            Parser parser = new(new ParserOptions
            {
                AllowDuplicates = false,
                Prefix = "--"
            });

            Assert.Throws<ArgumentParsingException>(() => parser.Parse(commandString, out string remainingText));
        }

        [Theory]
        [MemberData(nameof(DuplicateCommandString))]
        public void TestDuplicatePass(string commandString, string param1Name, string param2Name, string param2Value, List<object> param1Value)
        {
            Parser parser = new(new ParserOptions
            {
                AllowDuplicates = true,
                Prefix = "--"
            });

            IDictionary<string, object> arguments = parser.Parse(commandString, out string remainingText);
            Assert.Equal(string.Empty, remainingText);

            Assert.NotNull(arguments);
            Assert.Equal(2, arguments.Count);

            Assert.True(arguments.ContainsKey(param1Name));
            Assert.True(arguments.ContainsKey(param2Name));

            Assert.Equal(param2Value, arguments[param2Name]);

            Assert.IsType<List<object>>(arguments[param1Name]);
            List<object> list = (List<object>)arguments[param1Name];

            list.ForEach(o =>
            {
                Assert.IsType<string>(o);
            });

            for (int i = 0; i < param1Value.Count; i++)
            {
                Assert.Equal(param1Value[i], list[i]);
            }
        }

        [Theory]
        [MemberData(nameof(DuplicateCommandString))]
        public void TestDuplicateMapping(string commandString, string param1Name, string param2Name, string param2Value, List<object> param1Value)
        {
            Parser parser = new(new ParserOptions
            {
                AllowDuplicates = true,
                Prefix = "--"
            });

            MockArgument mock = parser.MapArguments<MockArgument>(commandString, out string remainingText);
            Assert.Equal(string.Empty, remainingText);

            Assert.NotNull(mock);
            Assert.NotNull(mock.Numbers);
            Assert.Equal(param1Value.Count + 1, mock.Numbers.Length);

            for (int i = 0; i < param1Value.Count; i++)
            {
                Assert.Equal(mock.Numbers[i + 1], Convert.ToInt32(param1Value[i]));
            }
        }

        [Fact]
        public void TheBigTest()
        {
            string text = @"--name Marin --series ""Dress-up Darling"" --name ""Marin /Kitagawa"" /series=whatever  /series=""asdf1""   /series   asdfsdk /justaflag /name ""asdf"" /series   'jiohnoi' /series 'John, the ""Fisherman""?' /name=John:,_/F321isherman? asdf";
            var dict = new Dictionary<string, object[]>(3)
            {
                { "name", new string[2] { "asdf1", "John:,_" } },
                { "series", new string[] { "asdfsdk", "jiohnoi", "John, the \"Fisherman\"?" } },
                { "justaflag", new object[] { true } }
            };

            Parser parser = new(new ParserOptions
            {
                AllowDuplicates = true,
                Prefix = "/"
            });

            var mock = parser.MapArguments<TheBigArgument>(text, out string remainingText);

            Assert.True(mock.Flag);
            Assert.Equal(5, mock.Series.Count);
            Assert.Equal(2, mock.Name.Count);
        }
    }
}