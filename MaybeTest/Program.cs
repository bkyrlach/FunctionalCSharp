using System;
using Functional.Monad;

namespace MaybeTest
{
    class GoodUser
    {
        public IMaybe<string> FirstName { get; private set; }
        public IMaybe<string> LastName { get; private set; }

        public GoodUser(string firstName = null, string lastName = null)
        {
            FirstName = firstName.ToMaybe();
            LastName = lastName.ToMaybe();
        }
    }

    class BadUser
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public BadUser(string firstName = null, string lastName = null)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var something = "Ben".ToMaybe();
            var nothing = MaybeOps.Nothing<string>();

            Console.WriteLine(
                something.Match(
                    just: s => s,
                    none: () => "Nothing here!"));

            Console.WriteLine(
                nothing.Match(
                just: s => s,
                none: () => "Nothing here!"));

            var a = new BadUser("Ben", "Kyrlach");
            var b = new BadUser("Adam");

            Console.WriteLine(FullName(a));  //Output is "Kyrlach, Ben"
            Console.WriteLine(FullName(b));  //Output is... null, Adam

            var c = new GoodUser("Ben", "Kyrlach");
            var d = new GoodUser("Adam");
            var e = new GoodUser("Bob", "Hope");

            Console.WriteLine(FullName(c));  //Output is "Some(Kyrlach, Ben)"
            Console.WriteLine(FullName(d));  //Output is Nothing

            Console.WriteLine(FullName(c).Where(s => s.Length > 10));
            Console.WriteLine(FullName(d).Where(s => s.Length > 10));
            Console.WriteLine(FullName(e).Where(s => s.Length > 10));

            var f = MaybeOps.Nothing<string>();
            var g = "abcdefg".ToMaybe();

            Console.WriteLine(f.Select(Length).Select(Square)); 
            Console.WriteLine(g.Select(Length).Select(Square));

            Length(null); //NPE!!!!

            Func<int, IMaybe<int>> halfIfEven = HalfIfEven;

            //Doesn't compile!
            //halfIfEven.Compose(halfIfEven);

            Console.WriteLine(10.ToMaybe().SelectMany(halfIfEven).SelectMany(halfIfEven));
            Console.WriteLine(20.ToMaybe().SelectMany(halfIfEven).SelectMany(halfIfEven));
        }

        static IMaybe<int> HalfIfEven(int n)
        {
            return n % 2 == 0 ? (n / 2).ToMaybe() : MaybeOps.Nothing<int>();
        }

        static int Length(String s)
        {
            return s.Length;
        }

        static int Square(int x)
        {
            return x * x;
        }

        static IMaybe<String> FullName(GoodUser u)
        {
            return from fn in u.FirstName
                   from ln in u.LastName
                   select ln + ", " + fn;
        }

        static string FullName(BadUser u)
        {
            return u.LastName + ", " + u.FirstName;
        }
    }

}
