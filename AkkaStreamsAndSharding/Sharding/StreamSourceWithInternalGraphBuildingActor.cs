using System;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaStreamsAndSharding.Common;
using AkkaStreamsAndSharding.Streams;

namespace AkkaStreamsAndSharding.Sharding
{
    public class StreamSourceWithInternalGraphBuildingActor : ReceiveActor
    {
        private readonly int _instrumentId;
        private (ISourceQueueWithComplete<Tick>, Source<Tick, NotUsed>) _source;

        public StreamSourceWithInternalGraphBuildingActor()
        {
            if (!int.TryParse(Self.Path.Name, out _instrumentId))
                throw new ApplicationException($"Invalid path: {Self.Path.ToStringWithUid()}");

            Receive<Tick>(async t => await ProcessTick(t));
            Receive<GraphMessage>(HandleGraphSourceMessage);
            Receive<StopMessage>(_ => Context.Parent.Tell(new Passivate(PoisonPill.Instance)));
        }

        private void HandleGraphSourceMessage(GraphMessage graph)
        {
            _source = graph.Source;
        }

        private async Task ProcessTick(Tick tick)
        {
            if (_source == (null, null))
            {
                return;
            }

            await _source.Item1.OfferAsync(tick);
        }

        protected override void PreStart()
        {
            ActorMaterializer MaterializerFactory() => Context.Materializer();
            
            new TaskFactory().StartNew(() =>
            {
                var source = GraphBuilder.BuildAndRunGraph(MaterializerFactory);

                Context.Self.Tell(new GraphMessage(_instrumentId, source));
            }, TaskCreationOptions.LongRunning);
        }

        protected override void PostStop()
        {
            CompleteGraph();
            base.PostStop();
        }

        private void CompleteGraph()
        {
            if (_source == (null, null))
                return;

            _source.Item1.Complete();
            _source.Item1.WatchCompletionAsync().Wait();
            _source = (null, null);
        }
    }
}
