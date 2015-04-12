using System;
using System.Linq;

namespace ResultMonad
{
    class Program
    {
        static void Main(string[] args)
        {
            // refer: http://ericlippert.com/2013/03/18/monads-part-eight/
            // refer: http://enterprisecraftsmanship.com/2015/03/20/functional-c-handling-failures-input-errors/
            // unit, wrap, do not double wrap
            // bind -- "apply", dude called it OnSuccess(

            var test = Result<int>.Ok(3);

            var result = test.OnSuccess(Result<int>.Ok)
                .OnSuccess(i =>
                {
                    Console.WriteLine(i);
                    return Result<int>.Ok(i);
                })
                .OnFail(i =>
                {
                    Console.WriteLine("should not run!");
                    return Result<int>.Ok(i);
                })
                .OnSuccess(() => Result<int>.Fail("fail!"))
                .OnSuccess(i =>
                {
                    Console.WriteLine("is failed already");
                    return Result<int>.Ok(i);
                })
                .OnFail(() =>
                {
                    Console.WriteLine("should run!");
                    return Result<string>.Fail("failed and now a string");
                })
                .OnBoth(() =>
                {
                    Console.WriteLine("should totes run!");
                    return Result<string>.Ok("a new value");
                })
                ;

            Console.WriteLine(result.Value);
            Console.WriteLine(result.Message);


            var x = Result<string>.Ok("value");
            // select and where
            var xx = from l in x 
                     where l.Equals("value")
                     select l + " and something" ;
            
            Console.WriteLine(xx.Value);
            // select many
            var xxx = 
                from l in x
                from m in xx
                select l + m;

            Console.WriteLine(xxx.Value);
            Console.ReadKey();
        }
    }

    // refer: http://enterprisecraftsmanship.com/2015/03/20/functional-c-handling-failures-input-errors/
    public class Result<T>
    {
        public Result(T value, bool isFailed, string message)
        {
            Value = value;
            IsFailed = isFailed;
            Message = message;
        }

        public T Value { get; private set; }

        public bool IsFailed { get; private set; }

        public string Message { get; private set; }

        public static Result<T> Ok(T value)
        {
            return value is Result<T>
                ? value as Result<T>
                : new Result<T>(value, false, "");
        }

        public static Result<T> Fail()
        {
            return Fail("");
        }

        public static Result<T> Fail(string message)
        {
            return new Result<T>(default(T), true, message);
        }
    }

    public static class ResultExtensions
    {
        public static Result<TResult> OnSuccess<T, TResult>(this Result<T> source, Func<Result<TResult>> function)
        {
            return !source.IsFailed
                ? function()
                : Result<TResult>.Fail();
        }

        public static Result<TResult> OnSuccess<T, TResult>(this Result<T> source, Func<T, Result<TResult>> function)
        {
            return !source.IsFailed
                ? function(source.Value)
                : Result<TResult>.Fail();
        }

        public static Result<TResult> OnFail<T, TResult>(this Result<T> source, Func<Result<TResult>> function)
        {
            return source.IsFailed
                ? function()
                : Result<TResult>.Fail();
        }

        public static Result<TResult> OnFail<T, TResult>(this Result<T> source, Func<T, Result<TResult>> function)
        {
            return source.IsFailed
                ? function(source.Value)
                : Result<TResult>.Fail();
        }

        public static Result<TResult> OnBoth<T, TResult>(this Result<T> source, Func<Result<TResult>> function)
        {
            return function();
        }

        public static Result<TResult> OnBoth<T, TResult>(this Result<T> source, Func<T, Result<TResult>> function)
        {
            return function(source.Value);
        }
    }

    public static class ResultLinqExtensions
    {
        private static Result<T> ToResult<T>(this T value, bool isFailed = false, string message = "")
        {
            return value is Result<T>
                ? value as Result<T>
                : new Result<T>(value, isFailed, message);
        }

        // refer: http://mikehadlow.blogspot.com.au/2011/01/monads-in-c-4-linq-loves-monads.html
        // refer: http://blogs.msdn.com/b/pfxteam/archive/2013/04/03/tasks-monads-and-linq.aspx
        // refers to the spec 
        // and refer to the System.Linq.Enumerable source
        /*
         * 7.16.3 The query expression pattern
         * The Query expression pattern establishes a pattern of methods that types can implement to support query expressions. 
         * Because query expressions are translated to method invocations by means of a syntactic mapping, types have 
         * considerable flexibility in how they implement the query expression pattern. For example, the methods of 
         * the pattern can be implemented as instance methods or as extension methods because the two have the same 
         * invocation syntax, and the methods can request delegates or expression trees because anonymous functions 
         * are convertible to both.
         * 
         * The recommended shape of a generic type C<T> that supports the query expression pattern is shown below. A 
         * generic type is used in order to illustrate the proper relationships between parameter and result types, but 
         * it is possible to implement the pattern for non-generic types as well.
         * 
         * delegate R Func<T1,R>(T1 arg1);
         * delegate R Func<T1,T2,R>(T1 arg1, T2 arg2);
         * 
         * class C
         * {
         *      public C<T> Cast<T>();
         * }
         * 
         * class C<T> : C
         * {
         * public C<T> Where(Func<T,bool> predicate);
         * public C<U> Select<U>(Func<T,U> selector);
         * public C<V> SelectMany<U,V>(Func<T,C<U>> selector, Func<T,U,V> resultSelector);
         * public C<V> Join<U,K,V>(C<U> inner, Func<T,K> outerKeySelector, Func<U,K> innerKeySelector, Func<T,U,V> resultSelector);
         * public C<V> GroupJoin<U,K,V>(C<U> inner, Func<T,K> outerKeySelector, Func<U,K> innerKeySelector, Func<T,C<U>,V> resultSelector);
         * public O<T> OrderBy<K>(Func<T,K> keySelector);
         * public O<T> OrderByDescending<K>(Func<T,K> keySelector);
         * public C<G<K,T>> GroupBy<K>(Func<T,K> keySelector);
         * public C<G<K,E>> GroupBy<K,E>(Func<T,K> keySelector, Func<T,E> elementSelector);
         * }
         * 
         * class O<T> : C<T>
         * {
         * public O<T> ThenBy<K>(Func<T,K> keySelector);
         * public O<T> ThenByDescending<K>(Func<T,K> keySelector);
         * }
         * 
         * class G<K,T> : C<T>
         * {
         *      public K Key { get; }
         * }
         * 
         * The methods above use the generic delegate types Func<T1, R> and Func<T1, T2, R>, but they 
         * could equally well have used other delegate or expression tree types with the same relationships 
         * in parameter and result types.
         * 
         * Notice the recommended relationship between C<T> and O<T> which ensures that the ThenBy and 
         * ThenByDescending methods are available only on the result of an OrderBy or OrderByDescending. Also notice 
         * the recommended shape of the result of GroupBy—a sequence of sequences, where each inner sequence has 
         * an additional Key property.
         * 
         * The System.Linq namespace provides an implementation of the query operator pattern for any type 
         * that implements the System.Collections.Generic.IEnumerable<T> interface.
         */

        public static Result<TResult> Select<T, TResult>(this Result<T> source, Func<T, TResult> selector)
        {
            return selector(source.Value).ToResult();
        }

        public static Result<TResult> SelectMany<T, B, TResult>(this Result<T> source, Func<T, Result<B>> selector, Func<T, B, TResult> resultSelector)
        {
            return source.OnSuccess(
              outer => selector(outer).OnSuccess<B, TResult>(
                  inner => resultSelector(outer, inner).ToResult()));
        }

        public static Result<T> Where<T>(this Result<T> source, Func<T, bool> predicate)
        {
            var result = predicate(source.Value);

            return result ? source : Result<T>.Fail();
        }
    }
}

