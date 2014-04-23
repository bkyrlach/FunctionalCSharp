using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Helper;

namespace FunctionComposition
{
    class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    class Program
    {
        private static readonly List<User> Users = new List<User>
        {
            new User {Id = 1, FirstName = "Ben", LastName = "Kyrlach"},
            new User {Id = 2, FirstName = "John", LastName = "Doe"},
            new User {Id = 3, FirstName = "Jane", LastName = "Doe"}
        };

        static void Main(string[] args)
        {
            Func<int, int> plus1 = n => n + 1;
            Func<string, int> length = s => s.Length;

            //g(x) = x + 1 = plus1
            //f(x) = x.length = length
            //g compose f == g(f(x))
            plus1.Compose(length);
            //f compose g == compiler error... type mismatch
            length.Compose(plus1);

            var ids = new List<int> { 1, 2 };

            ids.ForEach(Console.WriteLine);

            ids.Select(GetUserById).Select(Fullname).Select(length);

            //Sadly, compile error...
            ids.Select(length.Compose(Fullname).Compose(GetUserById));

            Func<User, string> fullname = Fullname;
            Func<int, User> getUserById = GetUserById;

            ids.Select(length.Compose(fullname).Compose(getUserById));
        }

        static string Fullname(User user)
        {
            return user.FirstName + " " + user.LastName;
        }

        static User GetUserById(int id)
        {
            return Users.Single(user => user.Id == id);
        }
    }

}
