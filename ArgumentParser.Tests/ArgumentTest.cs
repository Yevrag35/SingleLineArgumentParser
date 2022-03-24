using ArgumentParser.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ArgumentParser.Tests
{
    public class ArgumentTest
    {
        public static IEnumerable<object[]> CommandStrings(int take, int skip) => new object[][]
        {
            new object[]
            {
                "condex.exe /name=\"whatup\"", "whatup"
            },
            new object[]
            {
                "asdf.jpg, name whatup", "whatup"
            }
        }.Skip(skip).Take(take);

        [Fact]
        public void ConstructorTest()
        {
            var parser = new Parser();
            Assert.NotNull(parser);
        }

        [Fact]
        public void OptionsTest()
        {
            var options = new ParserOptions();
            Assert.NotNull(options.Prefix);

            Assert.False(options.CaseSensitive);

            _ = new Parser(options);
        }

        [Theory]
        [MemberData(nameof(CommandStrings), 1, 0)]
        public void TestAutoMapping(string commandString, object expectedValue)
        {
            var parser = new Parser();
            var argument = parser.MapArguments<MockArgument>(commandString, out string remainingText);

            Assert.NotNull(argument);
            Assert.NotEqual(commandString, remainingText);

            Assert.Equal(expectedValue, argument.Name);
        }

        [Theory]
        [MemberData(nameof(CommandStrings), 1, 0)]
        public void TestAutoMappingPreLoad(string commandString, object expectedValue)
        {
            var parser = new Parser();
            var preLoad = new MockArgument();
            string guid = Guid.NewGuid().ToString();
            preLoad.Name = guid;   // Pre-define value to see if it overwrites.

            var argument = parser.MapArguments(preLoad, commandString, out string remainingText);

            Assert.NotNull(argument);
            Assert.NotEqual(commandString, remainingText);

            Assert.Equal(expectedValue, argument.Name);
            Assert.NotEqual(guid, argument.Name);
        }

        [Theory]
        [MemberData(nameof(CommandStrings), 1, 1)]
        public void TestAutoMappingFail(string commandString, object expectedValue)
        {
            var parser = new Parser();
            var argument = parser.MapArguments<MockArgument>(commandString, out string remainingText);

            Assert.NotNull(argument);
            Assert.Equal(commandString, remainingText);

            Assert.NotEqual(expectedValue, argument.Name);
        }
    }
}