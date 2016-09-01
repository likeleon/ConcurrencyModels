using System;
using System.Threading;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread = new Thread(() => Console.WriteLine("Hello from new thread"));
            thread.Start();
            Thread.Yield();
            Console.WriteLine("Hello from main thread");
            thread.Join();
        }
    }
}
