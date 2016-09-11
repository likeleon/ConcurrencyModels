using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WordCount
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        static Dictionary<string, int> CountWordsSequential(IEnumerable<string> pages)
        {
            return pages
                .SelectMany(p => GetWords(p))
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
        }

        static void CountWords(IEnumerable<string> pages)
        {
            pages.PartitionAll(100)
                .AsParallel()
                .Select(x => CountWordsSequential(x));
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

        static IEnumerable<string> GetWords(string text)
        {
            return text.Split(' ', ',', '.', ':', '\t');
        }

        static TimeSpan Time(Action action)
        {
            var start = DateTime.Now;
            action();
            return DateTime.Now - start;
        }
    }

    static class Exts
    {
        public static IEnumerable<T[]> PartitionAll<T>(this IEnumerable<T> source, int n)
        {
            return source
                .Select((x, i) => new { Value = x, Index = i })
                .GroupBy(x => x.Index / n)
                .Select(x => x.Select(v => v.Value).ToArray());
        }
    }
}
