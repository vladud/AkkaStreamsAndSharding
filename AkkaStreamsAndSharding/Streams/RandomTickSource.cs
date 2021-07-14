using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Streams;
using Akka.Streams.Stage;
using Akka.Util;
using AkkaStreamsAndSharding.Common;
using DotNetty.Common;

namespace AkkaStreamsAndSharding.Streams
{
    public class RandomTickSource : GraphStage<SourceShape<Tick>>
    {
        private readonly int _instrumentId;

        private sealed class Logic : GraphStageLogic
        {
            private readonly Queue<Tick> _queue;
            private readonly CancellationTokenSource _cts;

            public Logic(RandomTickSource stage, int instrumentId) : base(stage.Shape)
            {
                _queue = new Queue<Tick>();
                _cts = new CancellationTokenSource();
                new TaskFactory().StartNew(() =>
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        var tick = new Tick(instrumentId, ThreadLocalRandom.Current.NextDouble(), ThreadLocalRandom.Current.NextDouble());
                        _queue.Enqueue(tick);
                        Thread.Sleep(ThreadLocalRandom.Current.Next(1000, 10000));
                    }
                }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                SetHandler(stage.Out, 
                    onPull: () =>
                    {
                        while (_queue.Count == 0)
                        {
                            Thread.Sleep(1000);
                        }
                        
                        Push(stage.Out, _queue.Dequeue());
                        Console.WriteLine($"Pushing tick for InstrumentId={instrumentId}");
                    },
                    onDownstreamFinish: () =>
                    {
                        Console.WriteLine("OnDownstreamFinished.");
                        _cts.Cancel();
                    });
            }
        }

        public RandomTickSource(int instrumentId)
        {
            _instrumentId = instrumentId;
            Shape = new SourceShape<Tick>(Out);
        }

        private Outlet<Tick> Out { get; } = new Outlet<Tick>("RandomTickSource.out");

        public override SourceShape<Tick> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this, _instrumentId);
    }
}
