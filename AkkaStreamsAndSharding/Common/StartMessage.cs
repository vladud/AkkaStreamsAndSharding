namespace AkkaStreamsAndSharding.Common
{
    public class StartMessage : IHasCustomKey
    {
        public StartMessage(int instrumentId)
        {
            InstrumentId = instrumentId;
        }

        public int InstrumentId { get; }
        public object Key => InstrumentId;
    }
}
