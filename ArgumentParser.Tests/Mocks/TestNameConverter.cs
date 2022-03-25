using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgumentParser.Tests.Mocks
{
    public class TestNameConverter : ArgumentConverter<string>
    {
        public override string Convert(ArgumentAttribute attribute, object rawValue, string existingValue, bool hasExistingValue)
        {
            return "Something completely different";
        }
    }
}