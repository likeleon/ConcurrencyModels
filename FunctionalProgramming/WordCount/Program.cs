using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace WordCount
{
    class Program
    {
        static int MaxPages => 100000;
        static string Filename => @"C:\Users\likeleon\Downloads\enwiki-latest-pages-articles1.xml";

        static void Main(string[] args)
        {
            var start = DateTime.Now;
            GetPages(Filename).Take(MaxPages).CountWords();
            Console.WriteLine(DateTime.Now - start);
        }

        static IEnumerable<string> GetPages(string filename)
        {
            using (var reader = XmlReader.Create(filename))
            {
                while (reader.ReadToFollowing("page"))
                {
                    if (!reader.ReadToFollowing("text"))
                        continue;

                    yield return reader.ReadElementContentAsString();
                }
            }
        }
    }

    static class Exts
    {
        static int pagesCount;
        public static void CountWords(this IEnumerable<string> pages)
        {
            var dicts = pages.PartitionAll(100)
                .AsParallel()
                .Select(CountWordsSequential);

            var counts = new Dictionary<string, int>();
            foreach (var x in dicts.SelectMany(x => x))
            {
                int count;
                if (counts.TryGetValue(x.Key, out count))
                    counts[x.Key] = count + x.Value;
                else
                    counts[x.Key] = count;
            }
        }

        static IEnumerable<T[]> PartitionAll<T>(this IEnumerable<T> source, int n)
        {
            return source
                .Select((x, i) => new { Value = x, Index = i })
                .GroupBy(x => x.Index / n)
                .Select(x => x.Select(v => v.Value).ToArray());
        }

        static Dictionary<string, int> CountWordsSequential(this IEnumerable<string> pages)
        {
            Interlocked.Increment(ref pagesCount);
            Console.WriteLine($"#{pagesCount}. Counting pages");
            return pages
                .SelectMany(p => GetWords(p))
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
        }

        static IEnumerable<string> GetWords(string text)
        {
            return text.Split(' ', ',', '.', ':', '\t');
        }

        static Dictionary<string, int> MergeWith(Dictionary<string, int> a, Dictionary<string, int> b)
        {
            var ret = new Dictionary<string, int>(a);
            foreach (var x in b)
            {
                if (ret.ContainsKey(x.Key))
                    ret[x.Key] += x.Value;
                else
                    ret[x.Key] = x.Value;
            }
            return ret;
        }
    }
}
