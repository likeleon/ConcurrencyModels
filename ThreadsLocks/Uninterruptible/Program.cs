using System;
using System.Threading;

namespace Uninterruptible
{
    class Program
    {
        static void Main(string[] args)
        {
            var o1 = new object();
            var o2 = new object();

            var t1 = new Thread(() =>
            {
                try
                {
                    lock (o1)
                    {
                        Thread.Sleep(1000);
                        lock (o2) { }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("t1 interrupted");
                }
            });

            var t2 = new Thread(() =>
            {
                try
                {
                    lock (o2)
                    {
                        Thread.Sleep(1000);
                        lock (o1) { }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("t2 interrupted");
                }
            });

            t1.Start();
            t2.Start();
            t1.Interrupt();
            t2.Interrupt();
            t1.Join();
            t2.Join();
        }
    }
}
