using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WordCount
{
    class Program
    {
        static readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            var start = DateTime.Now;
            var pages = new Pages(100000, @"C:\Users\likeleon\Downloads\enwiki-latest-pages-articles1.xml");
            foreach (var page in pages)
            {
                var words = new Words(page.Text);
                foreach (var word in words)
                    CountWord(word);
            }
            Console.WriteLine($"Elapsed {DateTime.Now - start}.");
        }

        static void CountWord(string word)
        {
            if (_counts.ContainsKey(word))
                _counts[word] += 1;
            else
                _counts[word] = 1;
        }
    }

    class Page
    {
        public string Title { get; }
        public string Text { get; }

        public Page(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }

    class Pages : IEnumerable<Page>
    {
        readonly int _maxPages;
        readonly string _filename;

        public Pages(int maxPages, string filename)
        {
            _maxPages = maxPages;
            _filename = filename;
        }

        public IEnumerator<Page> GetEnumerator()
        {
            using (var reader = XmlReader.Create(_filename))
            {
                var pagesYield = 0;
                while (reader.ReadToFollowing("page") && pagesYield < _maxPages)
                {
                    if (!reader.ReadToDescendant("title"))
                        continue;
                    var title = reader.ReadElementContentAsString();

                    if (!reader.ReadToFollowing("text"))
                        continue;
                    var text = reader.ReadElementContentAsString();

                    yield return new Page(title, text);
                    ++pagesYield;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class Words : IEnumerable<string>
    {
        readonly string _text;

        public Words(string text)
        {
            _text = text;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _text.Split(' ', ',', '.', ':', '\t').AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
