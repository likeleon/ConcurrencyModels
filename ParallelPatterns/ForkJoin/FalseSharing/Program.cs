using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FalseSharing
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            //WithFalseSharing();
            WithoutFalseSharing();
            watch.Stop();
            Console.WriteLine($"{watch.Elapsed}");
        }

        static void WithFalseSharing()
        {
            var rand1 = new Random();
            var rand2 = new Random();
            var results1 = new int[20000000];
            var results2 = new int[20000000];
            Parallel.Invoke(() =>
            {
                for (int i = 0; i < results1.Length; ++i)
                    results1[i] = rand1.Next();
            }, () =>
            {
                for (int i = 0; i < results2.Length; ++i)
                    results2[i] = rand2.Next();
            });
        }

        static void WithoutFalseSharing()
        {
            int[] results1, results2;
            Parallel.Invoke(() =>
            {
                var rand1 = new Random();
                results1 = new int[20000000];
                for (int i = 0; i < results1.Length; ++i)
                    results1[i] = rand1.Next();
            }, () =>
            {
                var rand2 = new Random();
                results2 = new int[20000000];
                for (int i = 0; i < results2.Length; ++i)
                    results2[i] = rand2.Next();
            });
        }
    }
}
