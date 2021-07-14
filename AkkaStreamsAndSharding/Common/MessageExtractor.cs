using Akka.Cluster.Sharding;

namespace AkkaStreamsAndSharding.Common
{
    public class MessageExtractor : HashCodeMessageExtractor
    {
        public MessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
        {
        }

        public override string EntityId(object message)
        {
            return (message as IHasCustomKey)?.Key.ToString();
        }
    }
}