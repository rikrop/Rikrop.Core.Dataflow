using System.Threading;
using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Timeout(1000)]
    public class BalancedTransmitterTest
    {
        [Test]
        public void ShouldStopTakeFromSourceWhenMaximalThresholdReached()
        {
            const int maximum = 5;

            var blocker = new TaskCompletionSource<bool>();
            var source = new Source(maximum + 5);
            var target = Mock.Create<ITarget<int>>();
            Mock.Arrange(() => target.SendAsync(0))
                .IgnoreArguments()
                .Returns(blocker.Task);

            var transfer = new BalancedTransmitter<int>(source, target, Mock.Create<IExceptionHandler>(), 0, maximum);
            transfer.StartAsync().Wait(100);

            Assert.AreEqual(maximum, source.Count);
        }

        [Test]
        public void ShouldStartTakeFromSourceWhenMinimalThresholdReached()
        {
            const int minimum = 5;
            const int maximum = 10;

            var blocker = new TaskCompletionSource<bool>();
            var source = new Source(maximum + minimum);
            var target = Mock.Create<ITarget<int>>();

            Mock.Arrange(() => target.SendAsync(0)).IgnoreArguments().Returns(blocker.Task);

            var transfer = new BalancedTransmitter<int>(source, target, Mock.Create<IExceptionHandler>(), minimum, maximum);
            var task = Task.Run(()=>transfer.StartAsync());
            task.Wait(100);
            
            blocker.SetResult(true);

            SpinWait.SpinUntil(() => source.Count == minimum + maximum);

            Assert.AreEqual(minimum + maximum, source.Count);
        }

        private class Source : ISource<int>
        {
            private readonly int _maximum;
            public int Count;

            public Source(int maximum)
            {
                _maximum = maximum;
            }

            public Task<int> ReceiveAsync()
            {
                int num = Count++;
                if (num + 1 < _maximum)
                {
                    return Task.FromResult(num);
                }
                return new TaskCompletionSource<int>().Task;
            }
        }
    }
}