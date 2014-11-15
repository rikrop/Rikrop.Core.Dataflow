using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Build;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Framework.Monitoring;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Ignore]
    public class Test
    {
        [Test]
        public void TestMethod()
        {
            var source = new Source();

            var intQueue = new ItemsQueueContainer<int>();
            var doubleQueue = new ItemsQueueContainer<double>();
            var exceptionHandler = Mock.Create<IExceptionHandler>();

            var myWork = new MyWork();

            var counter = new MyCounter();

            var dataflow = DataflowBuilder
                .StartFrom(source)
                .TransmitWith(o => o.Custom(new MyTransmitterFactory()))
                .SplitBatch()
                .BuildTransformation(o => o.Transform(int.Parse)
                                           .Transform(x => x.ToString())
                                           .Transform(myWork))
                .SendToPipe<int>(new InfinityPipe<int>(intQueue))
                .TransmitWith(o => o.BalancedTransmitter(exceptionHandler, 0, 10)
                                    .SetCounter(counters => counters.InWorkItemsCount = counter))
                .RouteTo(condition => condition.Case(o => o%3 == 0)
                                               .Transform(o => o.ToString())
                                               .SendToTarget(new LoggingTarget<string>("0) ", 100)),

                         condition => condition.Case(o => o%3 == 1)
                                               .BroadcastTo(target2 => target2.SendToTarget(new LoggingTarget<int>("1) ", 200)),

                                                            target2 => target2.ToBatches(4, TimeSpan.FromMilliseconds(500), exceptionHandler)
                                                                              .SplitBatch()
                                                                              .Transform(o => (double) o)
                                                                              .Transform(o => (int) o)
                                                                              .Transform(o => (double) o)
                                                                              .SendToPipe<double>(new InfinityPipe<double>(doubleQueue))
                                                                              .TransmitWith(o => o.BalancedTransmitter(exceptionHandler, 0, 10))
                                                                              .BuildTransformation(o => o.ToBatches(4, TimeSpan.FromMilliseconds(500), exceptionHandler)
                                                                                                         .SplitBatch())
                                                                              .SendToTarget(new LoggingTarget<double>("2) "))),

                         condition => condition.Default()
                                               .SendToTarget(new LoggingTarget<int>("3) ")))
                .Build();

            dataflow.Start();

            Console.WriteLine("Started!");

            Task.Delay(4000).Wait();

            Console.WriteLine("I={0}, J={1}", counter.I, counter.J);
        }
    }

    public class MyCounter : IDecrementableCounter
    {
        public int I;
        public int J;
        
        public void Increment()
        {
            Interlocked.Increment(ref I);
        }

        public void Decrement()
        {
            Interlocked.Increment(ref J);
        }
    }

    public class MyWork : IWork<string, int>
    {
        public Task<int> ExecuteAsync(string item)
        {
            return Task.FromResult(int.Parse(item));
        }
    }

    public class MyTransmitterFactory : ITransmitterFactory<IReadOnlyCollection<string>, IReadOnlyCollection<string>>
    {
        public ITransmitter Create(ISource<IReadOnlyCollection<string>> source, ITarget<IReadOnlyCollection<string>> target)
        {
            return new SimpleTransmitter<IReadOnlyCollection<string>>(source, target, Mock.Create<IExceptionHandler>());
        }
    }

    public class LoggingTarget<TItem> : ITarget<TItem>
    {
        private readonly string _prefix;
        private readonly int _delay;

        public LoggingTarget(string prefix = "", int delay = 50)
        {
            _prefix = prefix;
            _delay = delay;
        }

        public async Task SendAsync(TItem item)
        {
            await Task.Delay(_delay);
            Console.WriteLine("{0}{1}, {2}", _prefix, item, item.GetType());
        }
    }

    //public class WorkTarget<TItem, TResult> : ITarget<T>

    public class Source : ISource<IReadOnlyCollection<string>>
    {
        private int _count;
        public Task<IReadOnlyCollection<string>> ReceiveAsync()
        {
            if (_count == 1)
            {
                return Task.FromResult<IReadOnlyCollection<string>>(null);
            }
            var items = Enumerable.Range(0, 10).Select(o => o.ToString()).ToList();
            _count++;

            return Task.FromResult<IReadOnlyCollection<string>>(items);
        }
    }
}