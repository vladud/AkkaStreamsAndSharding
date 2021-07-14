using System;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Streams
{
    public class GraphBuilder
    {
        public static void BuildAndRunGraph(Func<ActorMaterializer> materializerFactory, int instrumentId)
        {
            var source = new RandomTickSource(instrumentId);

            var stupidGraph = Source.FromGraph(source).Via(Flow.Create<Tick>().Where(t => t.Ask > t.Bid)).To(Sink.ForEach<Tick>(t => Console.WriteLine($"Valid tick for InstrumentId={t.InstrumentId}")));

            stupidGraph.Run(materializerFactory());
            Console.WriteLine($"Graph built for instrumentId={instrumentId}");
        }
    }
}