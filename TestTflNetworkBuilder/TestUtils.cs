using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTflNetworkBuilder
{
    internal class TestUtils
    {
        public static void SaveTestOutput(string filename, string json)
        {
#if WRITE_TEST_OUTPUT
            File.WriteAllText(filename, json);
#endif
        }

        public static void VerifyTestOutput(string filename, string json)
        {
            var expected = File.ReadAllText(Path.Combine("TestData/Expected-Results", filename));
            Assert.Equal(expected, json);
        }
    }
}
