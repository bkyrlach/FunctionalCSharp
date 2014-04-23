using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Helper;

namespace FunctionCurrying
{
    class Program
    {
        static String Repeat(int i, string s)
        {
            return i <= 1 ? s : s + Repeat(i - 1, s);
        }

        private static readonly List<string> RepeatableList = new List<string> { "foo", "bar", "baz", "ben", "bob" };

        static void Main(string[] args)
        {
            Func<int, string, string> repeat = Repeat;
            Console.WriteLine(repeat(5, "ben")); //benbenbenbenben

            //RepeatableList.Select(Repeat(Step1()));

            var secretSauce = repeat.Curry();
            var partiallyApplied = secretSauce(Step1());
            RepeatableList.Select(partiallyApplied).ToList().ForEach(Console.WriteLine);
        }

        static int Step1()
        {
            var r = new Random();
            return r.Next(2, 5);
        }
    }

}
