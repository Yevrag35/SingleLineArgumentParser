using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgumentParser.Tests.Mocks
{
    public class TheBigArgument
    { 
        [Argument("series")]
        internal List<string> Series { get; set; }

        [Argument("name")]
        internal List<string> Name { get; set; }

        [Argument("justaflag")]
        public bool Flag { get; set; }
    }

    public class MockArgument
    {
        [Argument("number")]
        public int[]? Numbers { get; set; } = new int[] { 99 };

        [Argument("thisName", "name")]
        //[ArgumentConverter(typeof(TestNameConverter))]
        public string Name { get; set; } = string.Empty;

        [Argument("series")]
        public string Series { get; set; } = string.Empty;

        [Argument("time")]
        public DateTime TheDate { get; private set; }
    }
}