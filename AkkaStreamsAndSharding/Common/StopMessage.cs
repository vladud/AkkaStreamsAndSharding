namespace AkkaStreamsAndSharding.Common
{
    public class StopMessage : IHasCustomKey
    {
        public StopMessage(int instrumentId)
        {
            InstrumentId = instrumentId;
        }

        public int InstrumentId { get; }
        public object Key => InstrumentId;
    }
}
