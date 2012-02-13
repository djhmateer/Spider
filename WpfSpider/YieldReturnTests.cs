using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WpfTest
{
    [TestFixture]
    public class YieldReturnTests
    {
        List<string> listOfString = new List<string>();

        public string Start(string startingSite)
        {
            listOfString.Add("a");
            listOfString.Add("b");
            listOfString.Add("c");
            listOfString.Add("d");
            listOfString.Add("e");
            return "ok";
        }

        public IEnumerable<string> GetNext()
        {
            foreach (var item in listOfString)
            {
                yield return item;
            }

        }

        [Test]
        public void MethodUnderTest_scenario_expectedbehaviour()
        {
            
        }
    }
}
