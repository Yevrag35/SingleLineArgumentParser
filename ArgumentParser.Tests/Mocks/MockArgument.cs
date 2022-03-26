using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgumentParser.Tests.Mocks
{
    public class MockArgument
    {
        [Argument("number")]
        public int[]? Numbers { get; set; } = new int[] { 99 };

        [Argument("thisName", "name")]
        //[ArgumentConverter(typeof(TestNameConverter))]
        public string Name { get; set; } = string.Empty;

        [Argument("time")]
        public DateTime TheDate { get; private set; }
    }
}