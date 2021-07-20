using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Sharding
{
    public class StreamSourceActor : ReceiveActor
    {
        private readonly IActorRef _graphBuildingRouter;
        private readonly int _instrumentId;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        public StreamSourceActor(IActorRef graphBuildingRouter)
        {
            if (!int.TryParse(Self.Path.Name, out _instrumentId))
                throw new ApplicationException($"Invalid path: {Self.Path.ToStringWithUid()}");
            _graphBuildingRouter = graphBuildingRouter;

            Receive<StartMessage>(_ =>
            {
                _log.Info($"I'm alive! InstrumentId={_instrumentId}");
            });
            Receive<StopMessage>(_ => Context.Parent.Tell(new Passivate(PoisonPill.Instance)));
        }

        protected override void PreStart()
        {
            _log.Info($"Asking for graph for {_instrumentId}");
            _graphBuildingRouter.Tell(new NeedGraphMessage(_instrumentId));
        }
    }
}
