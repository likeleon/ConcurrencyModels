using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WordCount
{
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
