namespace WordCount
{
    class PoisonPill : Page
    {
        public PoisonPill()
            : base(null, null)
        {
        }

        public override bool IsPoisonPill => true;
    }
}
