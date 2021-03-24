using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLogUtility.Timing
{
    public class TimeExpressionTest
    {
        public static void Run()
        {
            var test = new TimeExpressionTest();

            test.TestParse_InputPlain_ReturnPlain();
            test.TestParse_InputRange_ReturnRange();
            test.TestParse_InputModulus_ReturnModulus();

            test.TestParse_InputWilcard_ReturnWildcard();
            test.TestParse_InputComplex_ReturnComplex();
            test.TestParse_InputList_ReturnList();
        }

        public void TestParse_InputPlain_ReturnPlain()
        {
            var time = TimeExpression.Parse("12:34:56");
            
            AssertEqual(new[] { 12 }, time.Hours);
            AssertEqual(new[] { 34 }, time.Minutes);
            AssertEqual(new[] { 56 }, time.Seconds);
        }
        public void TestParse_InputRange_ReturnRange()
        {
            var time = TimeExpression.Parse("-3 : 10-15 : 55 -");

            AssertEqual(new[] { 0, 1, 2, 3 }, time.Hours);
            AssertEqual(new[] { 10, 11, 12, 13, 14, 15 }, time.Minutes);
            AssertEqual(new[] { 55, 56, 57, 58, 59 }, time.Seconds);
        }
        public void TestParse_InputModulus_ReturnModulus()
        {
            var time = TimeExpression.Parse("%5 : %15 + 3 : % 20 +15");

            AssertEqual(new[] { 0, 5, 10, 15, 20 }, time.Hours);
            AssertEqual(new[] { 3, 18, 33, 48 }, time.Minutes);
            AssertEqual(new[] { 15, 35, 55 }, time.Seconds);
        }

        public void TestParse_InputWilcard_ReturnWildcard()
        {
            var time = TimeExpression.Parse("**:00:00");

            AssertEqual(Enumerable.Range(0, 24), time.Hours);
            AssertEqual(new[] { 0 }, time.Minutes);
            AssertEqual(new[] { 0 }, time.Seconds);
        }
        public void TestParse_InputComplex_ReturnComplex()
        {
            var time = TimeExpression.Parse("1 : 5-20 %5+3 : 1");

            AssertEqual(new[] { 1 }, time.Hours);
            AssertEqual(new[] { 8, 13, 18 }, time.Minutes);
            AssertEqual(new[] { 1 }, time.Seconds);
        }
        public void TestParse_InputList_ReturnList()
        {
            var time = TimeExpression.Parse("1 : 1, %15+3, 5-20 %5+3 : 1");

            AssertEqual(new[] { 1 }, time.Hours);
            AssertEqual(new[] { 1, 3, 8, 13, 18, 33, 48 }, time.Minutes);
            AssertEqual(new[] { 1 }, time.Seconds);
        }

        private static void AssertEqual<T>(IEnumerable<T> expected, IEnumerable<T> fact)
        {
            if (!expected.SequenceEqual(fact))
            {
                throw new InvalidOperationException();
            }
        }
    }
}
