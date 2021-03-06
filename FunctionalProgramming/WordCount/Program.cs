﻿using System;
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
                AddOrUpdate(counts, x.Key, x.Value, (_, old) => old + x.Value);
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
            var ret = new Dictionary<string, int>();
            foreach (var word in pages.SelectMany(p => GetWords(p)))
                AddOrUpdate(ret, word, 1, (_, old) => old + 1);
            return ret;
        }

        static IEnumerable<string> GetWords(string text)
        {
            return text.Split(' ', ',', '.', ':', '\t');
        }

        static Dictionary<string, int> MergeWith(Dictionary<string, int> dict1, Dictionary<string, int> dict2)
        {
            var ret = new Dictionary<string, int>(dict1);
            foreach (var x in dict2)
                AddOrUpdate(ret, x.Key, x.Value, (_, old) => old + x.Value);
            return ret;
        }

        static void AddOrUpdate<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue value, Func<TKey, TValue, TValue> updater)
        {
            TValue oldValue;
            if (dict.TryGetValue(key, out oldValue))
                dict[key] = updater(key, oldValue);
            else
                dict[key] = value;
        }
    }
}
