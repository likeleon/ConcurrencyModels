namespace WordCount
{
    class Page
    {
        public string Title { get; }
        public string Text { get; }
        public virtual bool IsPoisonPill { get; }

        public Page(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}
