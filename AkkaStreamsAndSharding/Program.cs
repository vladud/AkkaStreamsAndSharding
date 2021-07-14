using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.Routing;
using Akka.Util.Internal;
using AkkaStreamsAndSharding.Common;
using AkkaStreamsAndSharding.Sharding;
using AkkaStreamsAndSharding.Streams;

namespace AkkaStreamsAndSharding
{
    class Program
    {
        private const int ShardsPerNodeFactor = 10;
        private static readonly Random Rnd = new Random();

        private static int? _port;
        private static bool _shouldSendTicks;
        private static int _numberOfGraphs;
        private static int _sleepInBetweenTicks;
        private static int _numberOfGraphBuilderRoutees;

        private static ActorSystem _shardActorSystem;
        private static IActorRef _shardRegion;

        private static ActorSystem _streamActorSystem;
        private static IActorRef _graphBuildingRouter;

        private static CancellationTokenSource _cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                _port = int.Parse(args[0]);
                Console.Title += _port;
            }

            _shouldSendTicks = bool.Parse(args.Length > 1 ? args[1] : ConfigurationManager.AppSettings["ShouldSendTicks"]);
            _numberOfGraphs = int.Parse(ConfigurationManager.AppSettings["NumberOfGraphs"]);
            _sleepInBetweenTicks = int.Parse(ConfigurationManager.AppSettings["SleepInBetweenTicks"]);
            _numberOfGraphBuilderRoutees = int.Parse(ConfigurationManager.AppSettings["NumberOfGraphBuilderRoutees"]);

            Init();

            Console.WriteLine("Press any key to stop cluster node");
            Console.ReadKey();

            await Stop();
        }

        private static void Init()
        {
            Console.WriteLine("Press any key to start cluster node");
            Console.ReadKey();

            InitGraphBuildingRouter();
            InitShardRegion();

            if (_shouldSendTicks)
            {
                Console.WriteLine("Press any key to start sending ticks");
                Console.ReadKey();

                Console.WriteLine("Sending ticks... valid ones will be displayed as they are received");

                new TaskFactory().StartNew(() =>
                 {
                     while (!_cts.IsCancellationRequested)
                     {
                         Enumerable.Range(0, _numberOfGraphs).ForEach(i => { _shardRegion.Tell(new Tick(i, Rnd.NextDouble(), Rnd.NextDouble())); });
                         Thread.Sleep(_sleepInBetweenTicks);
                     }
                 }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            //else
            //{
            //    await Task.CompletedTask;
            //}
        }

        private static async Task Stop()
        {
            _cts.Cancel();
            await _shardRegion.GracefulStop(TimeSpan.FromSeconds(5));
            var cluster = Akka.Cluster.Cluster.Get(_shardActorSystem);
            cluster.Leave(cluster.SelfAddress);
            await _shardActorSystem.Terminate();
        }

        private static void InitShardRegion()
        {
            var customAkkaConfig = ConfigurationFactory.Load();
            var shardingAkkaConfig = customAkkaConfig.GetConfig("custom.sharding");
            if (_port.HasValue)
            {
                shardingAkkaConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp {{ port = {_port} }}").WithFallback(shardingAkkaConfig);
            }
            _shardActorSystem = ActorSystem.Create("sharding-system", shardingAkkaConfig);

            var props = Props.Create(() => new StreamSourceActor(_graphBuildingRouter));

            var sharding = ClusterSharding.Get(_shardActorSystem);
            _shardRegion = sharding.Start(
                typeName: typeof(StreamSourceActor).Name,
                entityProps: props,
                settings: ClusterShardingSettings.Create(_shardActorSystem),
                messageExtractor: new MessageExtractor(Akka.Cluster.Cluster.Get(_shardActorSystem).Settings.SeedNodes.Count * ShardsPerNodeFactor));
        }

        private static void InitGraphBuildingRouter()
        {
            var customAkkaConfig = ConfigurationFactory.Load();
            var streamingAkkaConfig = customAkkaConfig.GetConfig("custom.streams");
            _streamActorSystem = ActorSystem.Create("streams-system", streamingAkkaConfig);

            var props = Props.Create(() => new GraphBuildingActor());

            var routerConfig = new ConsistentHashingGroup().WithHashMapping(x => (x as IHasCustomKey)?.Key);
            _graphBuildingRouter = _streamActorSystem.ActorOf(props.WithRouter(routerConfig), "GraphBuilder");

            for (var i = 0; i < _numberOfGraphBuilderRoutees; ++i)
                _graphBuildingRouter.Tell(new AddRoutee(Routee.FromActorRef(_streamActorSystem.ActorOf(props, $"Router{i}"))));
        }
    }
}
