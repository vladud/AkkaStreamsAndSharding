using System.Collections.Concurrent;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Stage;
using AkkaStreamsAndSharding.Common;

namespace AkkaStreamsAndSharding.Streams
{
    public class RandomTickSource : GraphStage<SourceShape<Tick>>
    {
        private readonly int _instrumentId;
        private readonly ConcurrentQueue<Tick> _queue;
        private readonly ILoggingAdapter _log;

        private sealed class Logic : GraphStageLogic
        {
            public Logic(RandomTickSource stage, int instrumentId, ConcurrentQueue<Tick> queue, ILoggingAdapter log) : base(stage.Shape)
            {
                SetHandler(stage.Out, 
                    onPull: () =>
                    {
                        Tick tick;
                        while (!queue.TryDequeue(out tick))
                        {
                            Push(stage.Out, new Tick(0, 0, 0));
                            return;
                        }

                        //var tick = new Tick(instrumentId, ThreadLocalRandom.Current.NextDouble(), ThreadLocalRandom.Current.NextDouble());

                        Push(stage.Out, tick);
                        log.Info($"Pushing tick for InstrumentId={instrumentId}");
                    },
                    onDownstreamFinish: () =>
                    {
                        log.Info("OnDownstreamFinished.");
                    });
            }
        }

        public RandomTickSource(int instrumentId, ConcurrentQueue<Tick> queue, ILoggingAdapter log)
        {
            _instrumentId = instrumentId;
            _queue = queue;
            _log = log;
            Shape = new SourceShape<Tick>(Out);
        }

        private Outlet<Tick> Out { get; } = new Outlet<Tick>("RandomTickSource.out");

        public override SourceShape<Tick> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this, _instrumentId, _queue, _log);
    }
}
