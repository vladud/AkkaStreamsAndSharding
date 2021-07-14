using System;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaStreamsAndSharding.Common;
using Hyperion.Internal;

namespace AkkaStreamsAndSharding.Sharding
{
    public class StreamSourceActor : ReceiveActor
    {
        private readonly IActorRef _graphBuildingRouter;
        private int _instrumentId;
        private (ISourceQueueWithComplete<Tick>, Source<Tick, NotUsed>) _source;

        public StreamSourceActor(IActorRef graphBuildingRouter)
        {
            if (!int.TryParse(Self.Path.Name, out _instrumentId))
                throw new ApplicationException($"Invalid path: {Self.Path.ToStringWithUid()}");
            _graphBuildingRouter = graphBuildingRouter;

            Receive<Tick>(async t => await ProcessTick(t));
            Receive<StopMessage>(_ => Context.Parent.Tell(new Passivate(PoisonPill.Instance)));

            BecomeStacked(() =>
            {
                Receive<GraphMessage>(HandleGraphSourceMessage);
            });
        }

        private void HandleGraphSourceMessage(GraphMessage graph)
        {
            Console.WriteLine($"Received graph for {graph.Key}");
            _source = graph.Source;
            UnbecomeStacked();
        }

        private async Task ProcessTick(Tick tick)
        {
            if (_source == (null, null))
            {
                Console.WriteLine("Source is not set yet and I received tick!!!");
                return;
            }

            await _source.Item1.OfferAsync(tick);
        }

        protected override void PreStart()
        {
            Console.WriteLine($"Asking for graph for {_instrumentId}");
            _graphBuildingRouter.Tell(new NeedGraphMessage(_instrumentId));
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
