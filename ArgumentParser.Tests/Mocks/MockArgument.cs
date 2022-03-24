using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgumentParser.Tests.Mocks
{
    public class MockArgument
    {
        [Argument("thisName", "name")]
        public string Name { get; set; } = string.Empty;
    }
}