namespace PokerTracker3000.Common
{
    public readonly struct OneOf<T1, T2> where T1 : class
                                         where T2 : class
    {
        public T1? First { get; init; }

        public T2? Second { get; init; }

        public bool IsFirst => First != default;

        public bool IsSecond => Second != default;
    }
}