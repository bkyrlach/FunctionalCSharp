using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functional.Value;

namespace Functional.Helper
{
    /// <summary>
    /// This class provides some basic operations for lambdas that are strangely missing from the .NET stdlib
    /// </summary>
    public static class FunctionHelpers
    {
        /// <summary>
        /// An implementation of the C# keyword "if" that allows cleaner expressions than the ternary operator in
        /// some cases. If the predicate holds, then consequent, else alternative.
        /// </summary>
        /// <typeparam name="TResult">The type yielded from the consequent and alternative</typeparam>
        /// <param name="predicate">The predicate</param>
        /// <param name="consequent">The consequent</param>
        /// <param name="alternative">The alternative</param>
        /// <returns>The value yielded from consequent if predicate holds, else the value yielded from alternative</returns>
        public static TResult If<TResult>(bool predicate, Func<TResult> consequent, Func<TResult> alternative)
        {
            return predicate ? consequent() : alternative();
        }

        /// <summary>
        /// The standard identity function. Useful in functional programming, it just gives back the value presented.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="t">The value that you'll get back</param>
        /// <returns>The value you give it</returns>
        public static T Identity<T>(T t)
        {
            return t;
        }

        /// <summary>
        /// An abbreviation of identity, since we don't want to type out FunctionalHelpers.Identity everywhere
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T Id<T>(T t)
        {
            return t;
        }

        /// <summary>
        /// This method does type safe functional composition
        /// </summary>
        /// <typeparam name="TInitial"></typeparam>
        /// <typeparam name="TIntermediate"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static Func<TInitial, TResult> Compose<TInitial, TIntermediate, TResult>(
            this Func<TIntermediate, TResult> f, Func<TInitial, TIntermediate> g)
        {
            return tInitial => f(g(tInitial));
        }

        public static Func<T, Unit> Compose<T>(this Action f, Action<T> g)
        {
            return t =>
            {
                g(t);
                f();
                return Unit.Only;
            };
        }

        public static Func<Unit, T> Compose<T>(this Func<T> f, Action g)
        {
            return u =>
            {
                g();
                return f();
            };
        }

        public static Func<T1, Func<T2, T3>> Curry<T1, T2, T3>(this Func<T1, T2, T3> f)
        {
            return t1 => t2 => f(t1, t2);
        }
    }
}
