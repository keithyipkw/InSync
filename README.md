InSync is a general purpose library providing easy ways to correctly write thread-safe code for .NET.

It is easy to forget to acquire correct locks before accessing variables in moderately large classes. It is also difficult to spot the errors in code review. InSync introduces the patterns popular in C++ to solve the problem.

# Features

- Enforce lock acquisition 
- Automatic lock release
- A built-in dead-lock free algorithm to acquire multiple locks
- Easy migration
- High performance

# Quick Start

In this example, we have a supposedly thread-safe class:

```C#
class QuickStart
{
    private int X;
    private int Y;
    private readonly object padLock = new object();

    public void Foo(int add)
    {
        lock (padLock)
        {
            X += 1;
            Y += add;
        }
    }

    public void Bar(int subtract)
    {
        // oops
        X -= 1;
        Y -= subtract;
    }
    
    public (int X, int Y) Get()
    {
        lock (padLock)
        {
            return (X, Y);
        }
    }
}
```

InSync prevents this kind of bug:

```C#
class QuickStart
{
    private class State
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    private readonly Synchronized<State> state = Synchronized.Create(new State());

    public void Foo(int z)
    {
        state.WithLock(s =>
        {
            s.X += 1;
            s.Y += z;
        });
    }

    public void Bar(int z)
    {
        // alternative style
        using (var guard = state.Lock())
        {
            guard.Value.X -= 1;
            guard.Value.Y -= z;
        }
    }

    public (int X, int Y) Get()
    {
        // passes the return value
        return state.WithLock(s =>
        {
            return (s.X, s.Y);
        });
    }

    private void ReusableMethod(State state)
    {
        // useful for AsyncSynchronized<T> and ReaderWriterSynchronized<T> because they are non-reentrant
        state.X += 1;
    }
}
```

# Usage

## Single Lock

There are 3 types of synchronization objects. The API is similar among them.

- `Synchronized<T>` which uses `Monitor`
- `AsyncSynchronized<T>` which uses `SemaphoreSlim`
- `ReaderWriterSynchronized<T>` which uses `ReaderWriterLockSlim`

They can be created by constructors or convenient factory methods. For example:

```C#
var s1 = Synchronized.Create(value1);
var s2 = Synchronized.Create(padLock, value2);
```

Without supplying an object as the lock, `Synchronized<T>` uses the value as the lock. In contrast, `AsyncSynchronized<T>` and `ReaderWriterSynchronized<T>` creates new locks.

For automatical releases, there are 2 styles to access the value inside a synchronization object, `WithLock` with closures and `Lock` with `using`.

```C#
public interface ISynchronized<T> where T : class
{
    void WithLock(Action<T> action);

    TResult WithLock<TResult>(Func<T, TResult> func);

    bool TryWithLock(Action<T> action);

    bool TryWithLock<TResult>(Func<T, TResult> func, out TResult result);

    GuardedValue<T> Lock();

    GuardedValue<T> TryLock();
}
```

For example, `WithLock` with closures:

```C#
private readonly Synchronized<List<int>> list = Synchronized.Create(new List<int>());

public void WithLock()
{
    list.WithLock(list =>
    {
        list.Add(0);
        Console.WriteLine("locking");
    });
}

public int WithLockReturn()
{
    return list.WithLock(list => list.FirstOrDefault());
}

public void TryWithLock()
{
    if (list.TryWithLock(list => list.Add(0)))
    {
        Console.WriteLine("added");
    }
}
```

and `Lock` with `using`:

```C#
private readonly Synchronized<List<int>> list = Synchronized.Create(new List<int>());

public int Guard()
{
    using (var guard = list.Lock())
    {
        return guard.Value.FirstOrDefault();
    }
}

public int TryGuard()
{
    using (var guard = list.TryLock())
    {
        if (guard != null)
        {
            Console.WriteLine("locking");
            return guard.Value.FirstOrDefault();
        }
        Console.WriteLine("not locked");
        return -1;
    }
}
```

For manual releases:

```C#
public interface IBareLock
{
    object BarelyLock();

    bool BarelyTryLock(out object value);
}

public interface IBareLock<T> : IBareLock where T : class
{
    new T BarelyLock();
    
    bool BarelyTryLock(out T value);
}
```

## Struct and Mutable Reference

`ValueContainer<T>` provides a way to store `struct` or mutate the value:

```C#
private readonly Synchronized<ValueContainer<int>> container = Synchronized.Create(new ValueContainer<int>());

public void Increase()
{
    container.WithLock(c =>
    {
        ++c.Value;
    });
}
```

## Multiple Locks

`MultiSync` provides easy ways to acquire multiple locks without deadlocks. It does not require any setup nor lock organizations. Fairness is thread-based and provided by the OS because `Thread.Yield` is used. Livelock may occur for a very short period under high contention. In such case, CPU power is wasted.

It uses the smart and polite method described in https://howardhinnant.github.io/dining_philosophers.html#Polite. Basically, it tries to acquire the locks one by one. If an acquisition fails, it releases all acquired locks. Before a blocking retry of the last acquisition, it yields to let other threads to process first.

```C#
private readonly Synchronized<List<int>> lock1 = Synchronized.Create(new List<int>());
private readonly Synchronized<List<int>> lock2 = Synchronized.Create(new List<int>());

public void UnorderedAcquisition()
{
    using (var guard = MultiSync.All(new[] { lock1, lock2 }))
    {
        var list1 = guard[0];
        var list2 = guard[1];
        list1.AddRange(list2);
    }
}
```

## Synchronization Token

For some reasons, if the style shown by `ResuableMethod` in the quick start is not preferred, it is still possible to enforce locking for methods at compile time:

```C#
private class WriteToken
{
    private WriteToken() { }

    /// <summary>
    /// This should be created once per object only.
    /// </summary>
    /// <returns></returns>
    public static WriteToken CreatePerObjectOnly() => new WriteToken();
}

private readonly AsyncSynchronized<WriteToken> writeToken = AsyncSynchronized.Create(WriteToken.CreatePerObjectOnly());

public async Task Foo()
{
    using (var w = await writeToken.LockAsync())
    {
        ReusableMethod(w.Value);
    }
}

public async Task Bar()
{
    using (var w = await writeToken.LockAsync())
    {
        ReusableMethod(w.Value);
    }
}

private void ReusableMethod(WriteToken token)
{
    // if (token == null)
    //     throw new ArgumentNullException(nameof(token));
    // some complicated code
}
```

It is better than purely relying on documenting the methods and hope that callers do right.

# Pitfalls

## Async with Synchronized\<T>

`await` must not be used between locking and unlocking by `Synchronized<T>`. `AsyncSynchronized<T>` should be used instead in the following example:

```C#
public void NotThreadSafe()
{
    list.WithLock(async list => // 1. Monitor.Enter
    {
        await Task.Delay(1);

        // 3. This may resume after Monitor.Exit

        list.Add(0);

    }); // 2. Monitor.Exit
}

public async Task Throw()
{
    using (var guard = list.Lock()) // Monitor.Enter
    {
        await Task.Delay(1);

        // This may resume in an unspecified thread

        guard.Value.Add(0);

    } // Monitor.Exit may be called in the unspecified thread
}
```

## Dispose() Throws!

`GuardedValue<T>` and `GuardedMultiValue<T>` may throw if releases of locks fail. `Dispose()` is not expected to throw exceptions or otherwise it results in crashes. Failures in releasing locks usually cause difficult to debug deadlocks later. It is even worst than immediate crashes. Generally, releasing locks should not throw too. Thus, bubbling up the exceptions is a lesser evil than sallowing them.

## Still not thread-safe

If a plain reference to a `Synchronized<T>` is written by a thread then read by another thread, it is still not thread-safe:

```C#
class Wrong
{
    private Synchronized<object> obj;

    public void Foo()
    {
        new Thread(() =>
        {
            obj?.WithLock(o =>
            {
                Console.WriteLine(o.ToString());
            });
        }).Start();

        obj = Synchronized.Create(new object());
    }
}
```

The variable needs some synchronizations. For the above exmaple, `volatile` is sufficient:

```C#
class Correct
{
    volatile Synchronized<object> obj;

    public void Foo()
    {
        new Thread(() =>
        {
            obj?.WithLock(o =>
            {
                Console.WriteLine(o.ToString());
            });
        }).Start();

        obj = Synchronized.Create(new object());
    }
}
```

However, `volatile` is error-prone for more complicated usages. `Synchronized<ValueContainer<T>>` is a better choice:

``` C#
class Correct
{
    private readonly Synchronized<ValueContainer<object>> obj = Synchronized.Create(new ValueContainer<object>());

    public void Foo()
    {
        new Thread(() =>
        {
            obj.WithLock(container =>
            {
                Console.WriteLine(container.Value?.ToString());
            });
        }).Start();

        obj.WithLock(container => container.Value = new object());
    }
}
```

## Not abort-safe

This library does not support `Thread.Abort()`. It may leave some locks being permanently locked.

# Performance

To view the full benchmark result, please visit https://keithyipkw.github.io/InSync/performance.html

The benchmark program was run with the environment:

- InSync 1.0.0
- Targeting .NET Core 2.1
- .NET SDK 5.0.101
- Windows 10
- Ryzen 3700X
    - fix clock at 4GHz
    - SMT was disabled 
- StopWatch resolution 100ns
- High process priority

### Single Lock

With no contentions, execution time of different methods of acquiring 1 lock was measured. Because of insufficient resolution of `StopWatch`, each method was repeated 200,000 times. The averages of time in nanoseconds were calculated. 100 times of such measurement were ran to get 100 averages for each method. Core 4 was used.

Method|Mean|STD
---|---|---
Loop overhead|0.502205|0.002618017
lock|10.48168|0.042191334
Synchronized.WithLock|11.074585|0.154348616
Synchronized.Lock|23.35174|1.227891217
SemaphoreSlim.Wait|26.62024|0.105064133
AsyncSynchronized.WithLock|35.361215|0.94974477
AsyncSynchronized.Lock|58.553095|1.554136994
SemaphoreSlim.WaitAsync|25.621235|0.277216726
AsyncSynchronized.WithLockAsync|80.763075|1.842565078
AsyncSynchronized.LockAsync|103.82279|1.143010283

### Multiple Locks

Similarly, that of acquiring 2 locks was measured.

Method|Mean|STD
---|---|---
Loop overhead|0.50222|0.004764833
lock|20.710595|0.181092486
Synchronized.Lock|39.91558|0.510229626
MultiSync.All monitor|176.746805|3.011581144
MultiSync.All monitor reuse array|167.6974|1.265423182
SemaphoreSlim.WaitAsync|50.941635|0.240256196
MultiSync.AllAsync semaphoreslim|370.783275|3.404508767

### Dining Philosophers

The original C++ code was rewritten in C# but with 10s eating time. For the 4-core case, 11 times was run using core 4, 5, 6 and 7.

![dining_core_4](https://user-images.githubusercontent.com/2862593/103096006-754f1b00-463d-11eb-8469-0abcb0f955a4.png)
