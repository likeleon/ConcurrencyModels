using System;
using System.Threading;

namespace Puzzle
{
    class Program
    {
        static void Main(string[] args)
        {
            bool answerReady = false;
            int answer = 0;
            var t1 = new Thread(() =>
            {
                answer = 42;
                answerReady = true;
            });
            var t2 = new Thread(() =>
            {
                if (answerReady)
                    Console.WriteLine($"The meaning of life is: {answer}");
                else
                    Console.WriteLine("I don't know the answer");
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }
    }
}
