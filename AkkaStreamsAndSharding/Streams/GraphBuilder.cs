using System;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Streams
{
    public class GraphBuilder
    {
        public static (ISourceQueueWithComplete<Tick>, Source<Tick, NotUsed>) BuildAndRunGraph(Func<ActorMaterializer> materializerFactory)
        {
            var source = Source.Queue<Tick>(100, OverflowStrategy.DropHead).PreMaterialize(materializerFactory());

            var stupidGraph = source.Item2.Via(Flow.Create<Tick>().Where(t => t.Ask > t.Bid)).To(Sink.ForEach<Tick>(t => Console.WriteLine($"Valid tick for InstrumentId={t.InstrumentId}")));

            stupidGraph.Run(materializerFactory());
            return source;
        }
    }
}