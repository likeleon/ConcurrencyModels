using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WordCount
{
    class Program
    {
        static int MaxPages => 100000;
        static string Filename => @"C:\Users\likeleon\Downloads\enwiki-latest-pages-articles1.xml";


        static void Main(string[] args)
        {
            var start = DateTime.Now;
            //RunSequential();
            RunProducerConsumer();
            Console.WriteLine($"Elapsed {DateTime.Now - start}.");
        }

        static void RunSequential()
        {
            var counts = new Dictionary<string, int>();
            foreach (var page in new Pages(MaxPages, Filename))
                page.Text.CountWord(counts);
        }

        static void RunProducerConsumer()
        {
            var pages = new BlockingCollection<Page>();
            var parser = new Thread(() => Parser.Parse(pages, MaxPages, Filename));
            parser.Start();

            var counts = new Dictionary<string, int>();
            var counter = new Thread(() => Counter.Count(pages, counts));
            counter.Start();

            parser.Join();
            pages.Add(new PoisonPill());
            counter.Join();
        }
    }

    static class Exts
    {
        public static void CountWord(this string text, Dictionary<string, int> counts)
        {
            foreach (var word in new Words(text))
            {
                if (counts.ContainsKey(word))
                    counts[word] += 1;
                else
                    counts[word] = 1;
            }
        }
    }

    class Parser
    {
        public static void Parse(BlockingCollection<Page> pages, int maxPages, string filename)
        {
            try
            {
                foreach (var page in new Pages(maxPages, filename))
                    pages.Add(page);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    class Counter
    {
        public static void Count(BlockingCollection<Page> pages, Dictionary<string, int> counts)
        {
            while (true)
            {
                var page = pages.Take();
                if (page.IsPoisonPill)
                    break;

                page.Text.CountWord(counts);
            }
        }
    }
}
