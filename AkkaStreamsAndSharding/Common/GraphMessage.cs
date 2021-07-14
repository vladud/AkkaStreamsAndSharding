using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace AkkaStreamsAndSharding.Common
{
    public class GraphMessage : IHasCustomKey
    {
        public GraphMessage(int instrumentId, (ISourceQueueWithComplete<Tick>, Source<Tick, NotUsed>) source)
        {
            InstrumentId = instrumentId;
            Source = source;
        }

        public int InstrumentId { get; }
        public (ISourceQueueWithComplete<Tick>, Source<Tick, NotUsed>) Source { get; }
        public object Key => InstrumentId;
    }
}
