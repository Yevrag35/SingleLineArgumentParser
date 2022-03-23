using System;

namespace ArgumentParser
{
    public class Parser
    {
        private const string REGEX_FORMAT = @"{0}(\w+)(?:$|(?:(?:\s|\=)(?:((?:\w+)|\"".+\""))))";
        private readonly ParserOptions _options;

        public Parser()
            : this(new ParserOptions())
        {
        }
        public Parser(ParserOptions options)
        {
            _options = options;
        }


    }
}
