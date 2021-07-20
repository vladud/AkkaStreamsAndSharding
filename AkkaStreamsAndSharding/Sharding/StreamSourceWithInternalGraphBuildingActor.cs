using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Streams;
using AkkaStreamsAndSharding.Common;
using AkkaStreamsAndSharding.Streams;

namespace AkkaStreamsAndSharding.Sharding
{
    public class StreamSourceWithInternalGraphBuildingActor : ReceiveActor
    {
        private readonly int _instrumentId;
        private bool _isStarted = false;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        public StreamSourceWithInternalGraphBuildingActor()
        {
            if (!int.TryParse(Self.Path.Name, out _instrumentId))
                throw new ApplicationException($"Invalid path: {Self.Path.ToStringWithUid()}");

            Receive<StartMessage>(_ =>
            {
                if (!_isStarted)
                {
                    ActorMaterializer MaterializerFactory() => Context.System.Materializer();
                    GraphBuilder.BuildAndRunGraph(MaterializerFactory, _log, _instrumentId);

                    _log.Info($"Actor started! InstrumentId={_instrumentId}");
                }
            });
            Receive<StopMessage>(_ => Context.Parent.Tell(new Passivate(PoisonPill.Instance)));
        }
    }
}
