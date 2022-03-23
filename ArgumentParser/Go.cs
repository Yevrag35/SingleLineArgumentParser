using System;
using System.Collections.Generic;
using System.Text;

namespace ArgumentParser
{
    internal class Go
    {
        [Argument("name", "n")]
        public string Name { get; set; }
    }
}
