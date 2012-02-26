using System;
using System.Collections;
using NUnit.Framework;

namespace WpfTest
{
    [TestFixture]
    public class YieldReturnTests
    {
        //immediate execution (eager)
        public IEnumerable PowerImmediate(int number, int howManyToShow)
        {
            var result = new int[howManyToShow];
            result[0] = number;
            for (int i = 1; i < howManyToShow; i++)
                result[i] = result[i - 1] * number;
            return result;
        }

        //deferred and lazy
        public IEnumerable PowerYieldLazy(int number, int howManyToShow)
        {
            int counter = 0;
            int result = 1;
            while (counter++ < howManyToShow)
            {
                result = result * number;
                yield return result;
            }
        }

        
        [Test]
        public void PowerImmediate_WhenPass2AndWant8Numbers_ReturnAnEnumerable()
        {
            IEnumerable listOfInts = PowerImmediate(2, 8);

            foreach (int i in listOfInts)
                Console.Write("{0} ", i);
        }

      

        //Does an IEnumerable have to use Yield to be deferred..essentially yes
        [Test]
        public void PowerYieldLazy_WhenPass2AndWant8Numbers_ReturnAnEnumerableOfIntsOneAtATime()
        {
            //deferred and lazy execution
            IEnumerable listOfInts = PowerYieldLazy(2, 8);

            foreach (int i in listOfInts)
                Console.Write("{0} ", i);
        }
    }
}
















        ////deferred but eager - unusual
        //public IEnumerable PowerYieldEager(int number, int howManyToShow)
        //{
        //    var result = new int[howManyToShow];
        //    result[0] = number;
        //    for (int i = 1; i < howManyToShow; i++)
        //        result[i] = result[i - 1] * number;

        //    foreach (var value in result)
        //        yield return value;
        //}


  //[Test]
  //      public void PowerYieldEager_WhenPass2AndWant8Numbers_ReturnAnEnumerableOfInts()
  //      {
  //          //deferred but eager execution..unusual to do this
  //          IEnumerable listOfInts = PowerYieldEager(2, 8);

  //          foreach (int i in listOfInts)
  //              Console.Write("{0} ", i);
  //      }