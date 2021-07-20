using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Util;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Streams
{
    public class GraphBuilder
    {
        private static readonly CancellationTokenSource _cts;
        private static readonly ConcurrentDictionary<int, ConcurrentQueue<Tick>> _queues = new ConcurrentDictionary<int, ConcurrentQueue<Tick>>();

        static GraphBuilder()
        {
            foreach (var i in Enumerable.Range(0, 1000))
            {
                _queues[i] = new ConcurrentQueue<Tick>();
            }
            _cts = new CancellationTokenSource();
            new TaskFactory().StartNew(() =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    foreach (var i in Enumerable.Range(0, 1000))
                    {
                        var instrumentId = i;
                        var tick = new Tick(instrumentId, ThreadLocalRandom.Current.NextDouble(), ThreadLocalRandom.Current.NextDouble());
                        var queue = _queues[instrumentId];
                        queue.Enqueue(tick);
                        _queues[i] = new ConcurrentQueue<Tick>();
                    }
                  
                    Task.Delay(1000).Wait();
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static void BuildAndRunGraph(Func<ActorMaterializer> materializerFactory, ILoggingAdapter log, int instrumentId)
        {
            var source = new RandomTickSource(instrumentId, _queues[instrumentId], log);

            var stupidGraph = Source.FromGraph(source).Via(Flow.Create<Tick>().Where(t => t.Ask > t.Bid)).To(Sink.ForEach<Tick>(
                 t => log.Info($"Valid tick for InstrumentId={t.InstrumentId}")
                ));

            stupidGraph.Run(materializerFactory());
        }
    }
}