# ResultMonad
playing aorund with monads and linq

refer: http://ericlippert.com/2013/03/18/monads-part-eight/

refer: http://enterprisecraftsmanship.com/2015/03/20/functional-c-handling-failures-input-errors/
<pre><code>
 static void Main(string[] args)
        {
            
            // unit, wrap, do not double wrap
            // bind -- "apply", dude called it OnSuccess(

            var test = Result&lt;int&gt;.Ok(3);

            var result = test.OnSuccess(Result&lt;int&gt;.Ok)
                .OnSuccess(i =&gt;
                {
                    Console.WriteLine(i);
                    return Result&lt;int&gt;.Ok(i);
                })
                .OnFail(i =&gt;
                {
                    Console.WriteLine("should not run!");
                    return Result&lt;int&gt;.Ok(i);
                })
                .OnSuccess(() =&gt; Result&lt;int&gt;.Fail("fail!"))
                .OnSuccess(i =&gt;
                {
                    Console.WriteLine("is failed already");
                    return Result&lt;int&gt;.Ok(i);
                })
                .OnFail(() =&gt;
                {
                    Console.WriteLine("should run!");
                    return Result&lt;string&gt;.Fail("failed and now a string");
                })
                .OnBoth(() =&gt;
                {
                    Console.WriteLine("should totes run!");
                    return Result&lt;string&gt;.Ok("a new value");
                })
                ;

            Console.WriteLine(result.Value);
            Console.WriteLine(result.Message);


            var x = Result&lt;string&gt;.Ok("value");
            var xx = from l in x 
                     where l.Equals("value")
                     select l + " and something" ;
            
            Console.WriteLine(xx.Value);
            Console.ReadKey();
        }
    }
    </code></pre>
