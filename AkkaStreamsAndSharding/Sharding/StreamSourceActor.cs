using System;
using Akka;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Sharding
{
    public class StreamSourceActor : ReceiveActor
    {
        private readonly IActorRef _graphBuildingRouter;
        private readonly int _instrumentId;

        public StreamSourceActor(IActorRef graphBuildingRouter)
        {
            if (!int.TryParse(Self.Path.Name, out _instrumentId))
                throw new ApplicationException($"Invalid path: {Self.Path.ToStringWithUid()}");
            _graphBuildingRouter = graphBuildingRouter;

            Receive<StartMessage>(_ => { Console.WriteLine($"I'm alive! InstrumentId={_instrumentId}"); });
            Receive<StopMessage>(_ => Context.Parent.Tell(new Passivate(PoisonPill.Instance)));
        }

        protected override void PreStart()
        {
            Console.WriteLine($"Asking for graph for {_instrumentId}");
            _graphBuildingRouter.Tell(new NeedGraphMessage(_instrumentId));
        }
    }
}
