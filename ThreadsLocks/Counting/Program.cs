using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Counting
{
    class Program
    {
        static void Main(string[] args)
        {
            //var counter = new Counter();
            //var counter = new CounterFixed();
            var counter = new CounterBetter();
            ThreadStart entry = () =>
            {
                for (var i = 0; i < 10000; ++i)
                    counter.Increment();
            };
            var t1 = new Thread(entry);
            var t2 = new Thread(entry);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Console.WriteLine(counter.Count);
        }
    }

    class Counter
    {
        public int Count { get; private set; }
        public void Increment() => ++Count;
    }

    class CounterFixed
    {
        public int Count { get; private set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Increment() => ++Count;
    }

    class CounterBetter
    {
        public int Count => Interlocked.CompareExchange(ref _counter, 0, 0);
        int _counter;

        public void Increment() => Interlocked.Increment(ref _counter);
    }
}
