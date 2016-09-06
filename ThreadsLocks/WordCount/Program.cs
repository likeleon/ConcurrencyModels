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
            //RunSequential(); // 32secs
            //RunProducerConsumer(); // 28secs
            //RunDictionary(Environment.ProcessorCount, DictionaryOptions.Synchronized); // 27 secs
            //RunDictionary(Environment.ProcessorCount, DictionaryOptions.Concurrent); // 23 secs
            RunDictionary(Environment.ProcessorCount, DictionaryOptions.BatchConcurrent); // 23 secs
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
            var counter = new Thread(() => Counter.Count(pages, counts, false));
            counter.Start();

            parser.Join();
            pages.Add(new PoisonPill());
            counter.Join();
        }

        enum DictionaryOptions
        {
            Synchronized,
            Concurrent,
            BatchConcurrent
        }

        static void RunDictionary(int numConsumers, DictionaryOptions dictOption)
        {
            var pages = new BlockingCollection<Page>();
            var parser = new Thread(() => Parser.Parse(pages, MaxPages, Filename));
            parser.Start();

            using (var countdown = new CountdownEvent(numConsumers))
            {
                if (dictOption == DictionaryOptions.Synchronized)
                    CountSynchronizedDictionary(pages, numConsumers, countdown);
                else if (dictOption == DictionaryOptions.Concurrent)
                    CountConcurrentDictionary(pages, numConsumers, countdown);
                else if (dictOption == DictionaryOptions.BatchConcurrent)
                    CountBatchConcurrentDictionary(pages, numConsumers, countdown);

                parser.Join();
                for (int i = 0; i < numConsumers; ++i)
                    pages.Add(new PoisonPill());
                countdown.Wait();
            }
        }

        static void CountSynchronizedDictionary(BlockingCollection<Page> pages, int numCounters, CountdownEvent countdown)
        {
            var counts = new Dictionary<string, int>();
            for (int i = 0; i < numCounters; ++i)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Counter.Count(pages, counts, true);
                    countdown.Signal();
                });
            }
        }

        static void CountConcurrentDictionary(BlockingCollection<Page> pages, int numCounters, CountdownEvent countdown)
        {
            var counts = new ConcurrentDictionary<string, int>();
            for (int i = 0; i < numCounters; ++i)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Counter.CountConcurrent(pages, counts);
                    countdown.Signal();
                });
            }
        }

        static void CountBatchConcurrentDictionary(BlockingCollection<Page> pages, int numCounters, CountdownEvent countdown)
        {
            var counts = new ConcurrentDictionary<string, int>();
            for (int i = 0; i < numCounters; ++i)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var localCounts = new Dictionary<string, int>();
                    Counter.Count(pages, localCounts, false);
                    foreach (var kvp in localCounts)
                        counts.AddOrUpdate(kvp.Key, kvp.Value, (k, oldValue) => oldValue + kvp.Value);
                    countdown.Signal();
                });
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
        public static void Count(BlockingCollection<Page> pages, Dictionary<string, int> counts, bool synchronized)
        {
            while (true)
            {
                var page = pages.Take();
                if (page.IsPoisonPill)
                    break;

                if (synchronized)
                    page.Text.CountWordWithLock(counts);
                else
                    page.Text.CountWord(counts);
            }
        }

        public static void CountConcurrent(BlockingCollection<Page> pages, ConcurrentDictionary<string, int> counts)
        {
            while (true)
            {
                var page = pages.Take();
                if (page.IsPoisonPill)
                    break;

                page.Text.CountWordConcurrentDict(counts);
            }
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

        public static void CountWordWithLock(this string text, Dictionary<string, int> counts)
        {
            lock (counts)
                CountWord(text, counts);
        }

        public static void CountWordConcurrentDict(this string text, ConcurrentDictionary<string, int> counts)
        {
            foreach (var word in new Words(text))
                counts.AddOrUpdate(word, 1, (_, oldValue) => oldValue + 1);
        }
    }
}
