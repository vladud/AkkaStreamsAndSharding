﻿using Akka.Actor;
using Akka.Streams;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Streams
{
    public class GraphBuildingActor : ReceiveActor
    {
        public GraphBuildingActor()
        {
            Receive<NeedGraphMessage>(HandleNeedGraphMessage);
        }

        private static void HandleNeedGraphMessage(NeedGraphMessage message)
        {
            ActorMaterializer MaterializerFactory() => Context.Materializer();

            var source = GraphBuilder.BuildAndRunGraph(MaterializerFactory);

            Context.Sender.Tell(new GraphMessage(message.InstrumentId, source));
        }
    }
}