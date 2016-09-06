using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace WordCount
{
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
}
