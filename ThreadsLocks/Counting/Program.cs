using System;
using System.Threading;

namespace Counting
{
    class Program
    {
        static void Main(string[] args)
        {
            var counter = new Counter();
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
}
