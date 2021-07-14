namespace AkkaStreamsAndSharding.Common
{
    public class NeedGraphMessage : IHasCustomKey
    {
        public NeedGraphMessage(int instrumentId)
        {
            InstrumentId = instrumentId;
        }

        public int InstrumentId { get; }
        public object Key => InstrumentId;
    }
}
