using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Monad;
using Functional.Value;
using F = Functional.Helper.FunctionHelpers;

namespace CoinFlipFP
{
    interface ICommand
    {
        T Match<T>(Func<T> bid, Func<T> quit, Func<T> unrecognized);
    }

    sealed class Bid : ICommand
    {
        public static readonly ICommand Command = new Bid();
        private Bid()
        {
        }

        public T Match<T>(Func<T> bid, Func<T> quit, Func<T> unrecognized)
        {
            return bid();
        }
    }

    sealed class Quit : ICommand
    {
        public static readonly ICommand Command = new Quit();
        private Quit()
        {
        }

        public T Match<T>(Func<T> bid, Func<T> quit, Func<T> unrecognized)
        {
            return quit();
        }
    }

    sealed class Unrecognized : ICommand
    {
        public static readonly ICommand Command = new Unrecognized();

        private Unrecognized()
        {

        }

        public T Match<T>(Func<T> bid, Func<T> quit, Func<T> unrecognized)
        {
            return unrecognized();
        }
    }

    interface IFace
    {

    }

    sealed class Heads : IFace
    {
        public static readonly IFace Value = new Heads();

        private Heads()
        {
        }
    }

    sealed class Tails : IFace
    {
        public static readonly IFace Value = new Tails();

        private Tails()
        {

        }
    }

    interface IGambleResult
    {
        T Match<T>(Func<T> winning, Func<T> losing);
    }

    sealed class Winning : IGambleResult
    {
        public static readonly IGambleResult Result = new Winning();
        private Winning()
        {

        }

        public T Match<T>(Func<T> winning, Func<T> losing)
        {
            return winning();
        }
    }

    sealed class Losing : IGambleResult
    {
        public static readonly IGambleResult Result = new Losing();

        private Losing()
        {

        }

        public T Match<T>(Func<T> winning, Func<T> losing)
        {
            return losing();
        }
    }

    sealed class Player
    {
        public String Name { get; private set; }
        public int Money { get; private set; }

        public Player(String name, int money = 100)
        {
            Name = name;
            Money = money;
        }
    }

    sealed class Bet
    {
        public int Amount { get; private set; }
        public IFace Face { get; private set; }

        public Bet(int amount, IFace face)
        {
            Amount = amount;
            Face = face;
        }
    }

    internal class Program
    {
        private static readonly Random r = new Random();

        private static IEnumerable<int> Rng(int min, int max)
        {
            while (true)
            {
                yield return r.Next(min, max + 1);   
            }            
        }

        private static void Main(string[] args)
        {
            (from _ in IoOps.Put("What is your name? ")
             from name in IoOps.GetLine()
             from __ in IoOps.PutLine(string.Format("Hello, {0}", name))
             from ___ in GameLoop(new Player(name: name), Rng(1, 2))
             select ___).UnsafePerformIo();
        }

        private static Io<Unit> GameLoop(Player p, IEnumerable<int> rngs)
        {
            return
                from _ in
                    IoOps.PutLine(string.Format("Hey {0}, looks like you want to make a bet with that juicy ${1} you're waving around!", p.Name, p.Money))
                from __ in IoOps.Put("Would you like to (B)id or (Q)uit? ")
                from c in Io<ICommand>.Apply(() => ParseCommand(Console.ReadLine()))
                from ___ in ExecuteCommand(c, p, rngs)
                select ___;
        }

        private static ICommand ParseCommand(String s)
        {
            var retval = Unrecognized.Command;
            if (s.Equals("b", StringComparison.CurrentCultureIgnoreCase))
            {
                retval = Bid.Command;
            }
            else if (s.Equals("q", StringComparison.CurrentCultureIgnoreCase))
            {
                retval = Quit.Command;
            }
            return retval;
        }

        private static Io<Unit> ExecuteCommand(ICommand c, Player p, IEnumerable<int> rngs)
        {
            Func<Io<Unit>> invalidCommand = () => from _ in IoOps.PutLine("I didn't recognize that command.")
                from __ in GameLoop(p, rngs)
                select __;

            return c.Match(
                unrecognized: invalidCommand,
                bid: () => from b in BidLoop()
                    from _ in AnnounceResult(b, PlaceBet(b, rngs))
                    from __ in HandleResult(b, PlaceBet(b, rngs), p, rngs)
                    select __,
                quit: invalidCommand);
        }

        static IFace Toss(IEnumerable<int> rngs)
        {
            return ((rngs.First() % 2) == 0) ? Heads.Value : Tails.Value;
        }

        static IGambleResult PlaceBet(Bet b, IEnumerable<int> rngs)
        {
            return b.Face == Toss(rngs) ? Winning.Result : Losing.Result;
        }

        private static Io<Unit> AnnounceResult(Bet b, IGambleResult r)
        {
            return r.Match(
                winning: () => IoOps.PutLine("You won $" + b.Amount + "! Congratulations!\n"),
                losing: () => IoOps.PutLine("You lost $" + b.Amount + "! Sucker!!!!\n"));
        }

        private static Io<Unit> HandleResult(Bet b, IGambleResult r, Player p, IEnumerable<int> rngs)
        {
            return r.Match(
                winning: () =>
                {
                    var newPlayer = new Player(name: p.Name, money: p.Money + b.Amount);
                    return GameLoop(newPlayer, rngs.Skip(1));
                },
                losing: () => {
                    var newPlayer = new Player(name: p.Name, money: p.Money - b.Amount);
                    return GameLoop(newPlayer, rngs.Skip(1));
                });
        }

        private static Io<Bet> BidLoop()
        {
            return from amt in AmountLoop()
                   from face in FaceLoop()
                   select new Bet(amt, face);
        }

        private static Io<int> AmountLoop()
        {
            return from _ in IoOps.Put("Amount to bet? ")
                from a in IoOps.GetLine()
                select
                    SafeParseInt(a)
                        .GetOrElse(() =>
                            IoOps.PutLine("Invalid amount there buddy. Try again!")
                                .SelectMany(x => AmountLoop())
                                .UnsafePerformIo());
        }

        private static Io<IFace> FaceLoop()
        {
            return from _ in IoOps.Put("(H)eads or (T)ails? ")
                from f in IoOps.GetLine()
                select
                    SafeStringToFace(f)
                        .GetOrElse(() => IoOps.PutLine("Make a real call!").SelectMany(x => FaceLoop()).UnsafePerformIo());
        }

        private static IMaybe<IFace> SafeStringToFace(String s)
        {
            var retval = MaybeOps.ToMaybe<IFace>(null);
            if (s.Equals("h", StringComparison.CurrentCultureIgnoreCase))
            {
                retval = Heads.Value.ToMaybe();
            }
            else if (s.Equals("t", StringComparison.CurrentCultureIgnoreCase))
            {
                retval = Tails.Value.ToMaybe();
            }
            return retval;
        }

        private static IMaybe<int> SafeParseInt(String s)
        {
            var retval = MaybeOps.Nothing<int>();
            int n;
            var success = int.TryParse(s, out n);
            if (success)
            {
                retval = n.ToMaybe();
            }
            Console.WriteLine(retval);
            Console.WriteLine(n);
            return retval;
        }
    }
}
