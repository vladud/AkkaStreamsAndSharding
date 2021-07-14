namespace AkkaStreamsAndSharding.Common
{
    public class Tick : IHasCustomKey
    {
        public int InstrumentId { get; }
        public double Ask { get; }
        public double Bid { get; }

        public Tick(int instrumentId, double ask, double bid)
        {
            InstrumentId = instrumentId;
            Ask = ask;
            Bid = bid;
        }

        public object Key => InstrumentId;
    }
}
