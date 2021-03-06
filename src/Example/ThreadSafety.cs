﻿using InSync;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Example
{
    class ThreadSafety
    {
        class Wrong
        {
            private Synchronized<object>? obj;

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

        class Correct
        {
            volatile Synchronized<object>? obj; // volatile is sufficient for this usage

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

        class Correct2
        {
            private readonly Synchronized<ValueContainer<object>> obj = Synchronized.Create(new ValueContainer<object>(new object()));

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
    }
}
