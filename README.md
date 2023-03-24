# Lazy Exception Behavior

[Exceptions in Lazy Objects](https://learn.microsoft.com/en-us/dotnet/framework/performance/lazy-initialization#exceptions-in-lazy-objects)

##

The `System.Lazy<T>` type defaults to `ExecutionAndPublication` mode, which caches any exception thrown by the value factory delegate. A common scenario is that the value factory involves code that may fail intermittently (e.g., a network request) but which is intended to be retried until the value factory runs successfully. The alternative `LazyThreadSafetyMode.PublicationOnly` mode can support this use case, but it also changes the behavior and correctness of the application.

`ExecutionAndPublication` works like taking a lock on the entire initialization step, then saving the result. If ten threads come in trying to access the Lazy, only one gets to run the initializer, and the other nine read the value (or the exception, if it fails). The initializer never runs twice.

Using `PublicationOnly` works by letting the initializer run multiple times then having threads race to complete and store the first successful result. If ten threads come in trying to access the Lazy, the initializer gets run ten times in parallel, and the first thread to finish running it without an error will save the value, while the other nine throw away their result. Subsequent threads will not rerun the initializer, only read the saved value from the thread that won the race.

This isn’t a safe value to change unless the initialization logic within the value factory delegate is thread-safe, because it will be run in parallel if multiple threads attempt to access the uninitialized Lazy simultaneously.

### Exception Behavior

`PublicationOnly` mode also involves different exception behavior. If out of the ten threads running in parallel, seven succeed and three encounter an exception, the failed threads will still raise that exception out of the Lazy value.

This repository contains a demonstration of this effect.

#### Example output

```
2 succeeded with value: lazy-48
6 succeeded with value: lazy-48
0 succeeded with value: lazy-48
5 succeeded with value: lazy-48
1 got exception System.InvalidOperationException: Failed lazy-69
   at Program.<>c__DisplayClass0_0.<<Main>$>b__0() in S:\Work\lazy_test\Program.cs:line 7
   at System.Lazy`1.CreateValue()
   at System.Lazy`1.get_Value()
   at Program.<>c__DisplayClass0_1.<<Main>$>b__1(Int32 n) in S:\Work\lazy_test\Program.cs:line 14
4 got exception System.InvalidOperationException: Failed lazy-84
   at Program.<>c__DisplayClass0_0.<<Main>$>b__0() in S:\Work\lazy_test\Program.cs:line 7
   at System.Lazy`1.CreateValue()
   at System.Lazy`1.get_Value()
   at Program.<>c__DisplayClass0_1.<<Main>$>b__1(Int32 n) in S:\Work\lazy_test\Program.cs:line 14
3 got exception System.InvalidOperationException: Failed lazy-57
   at Program.<>c__DisplayClass0_0.<<Main>$>b__0() in S:\Work\lazy_test\Program.cs:line 7
   at System.Lazy`1.CreateValue()
   at System.Lazy`1.get_Value()
   at Program.<>c__DisplayClass0_1.<<Main>$>b__1(Int32 n) in S:\Work\lazy_test\Program.cs:line 14
8 got exception System.InvalidOperationException: Failed lazy-81
   at Program.<>c__DisplayClass0_0.<<Main>$>b__0() in S:\Work\lazy_test\Program.cs:line 7
   at System.Lazy`1.CreateValue()
   at System.Lazy`1.get_Value()
   at Program.<>c__DisplayClass0_1.<<Main>$>b__1(Int32 n) in S:\Work\lazy_test\Program.cs:line 14
9 got exception System.InvalidOperationException: Failed lazy-80
   at Program.<>c__DisplayClass0_0.<<Main>$>b__0() in S:\Work\lazy_test\Program.cs:line 7
   at System.Lazy`1.CreateValue()
   at System.Lazy`1.get_Value()
   at Program.<>c__DisplayClass0_1.<<Main>$>b__1(Int32 n) in S:\Work\lazy_test\Program.cs:line 14
7 got exception System.InvalidOperationException: Failed lazy-96
   at Program.<>c__DisplayClass0_0.<<Main>$>b__0() in S:\Work\lazy_test\Program.cs:line 7
   at System.Lazy`1.CreateValue()
   at System.Lazy`1.get_Value()
   at Program.<>c__DisplayClass0_1.<<Main>$>b__1(Int32 n) in S:\Work\lazy_test\Program.cs:line 14
```
