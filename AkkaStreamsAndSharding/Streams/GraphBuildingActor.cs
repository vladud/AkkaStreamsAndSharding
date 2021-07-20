using Akka.Actor;
using Akka.Event;
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

            GraphBuilder.BuildAndRunGraph(MaterializerFactory, Context.GetLogger(), message.InstrumentId);
        }
    }
}
