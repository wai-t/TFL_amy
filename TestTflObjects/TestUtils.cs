using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTflObjects
{
    internal class TestUtils
    {
        public static void SaveTestOutput(string filename, string json)
        {
#if WRITE_TEST_OUTPUT
            File.WriteAllText(filename, json);
#endif
        }
    }
}
