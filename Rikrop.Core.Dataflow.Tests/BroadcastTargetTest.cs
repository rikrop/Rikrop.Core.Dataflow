using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Timeout(1000)]
    public class BroadcastTargetTest
    {
        private ActResult ArrangeAndAct(BroadcastMethod broadcastMethod, bool sendInThreadPool)
        {
            var target1 = Mock.Create<ITarget<string>>();
            var target2 = Mock.Create<ITarget<string>>();

            var sw = new Stopwatch();

            var secondTargetRunnedAfter = TimeSpan.Zero;
            var firstTargetThreadId = 0;
            var secondTargetThreadId = 0;
            Mock.Arrange(() => target1.SendAsync("1")).Returns(() =>
                                                                   {
                                                                       firstTargetThreadId = Thread.CurrentThread.ManagedThreadId;
                                                                       return Task.Delay(100);
                                                                   });
            Mock.Arrange(() => target2.SendAsync("1")).Returns(() =>
                                                                   {
                                                                       secondTargetThreadId = Thread.CurrentThread.ManagedThreadId;
                                                                       secondTargetRunnedAfter = sw.Elapsed;
                                                                       return Task.FromResult(true);
                                                                   });

            var bTarget = new BroadcastingTarget<string>(new[] {target1, target2}, broadcastMethod, sendInThreadPool);

            sw.Start();
            bTarget.SendAsync("1").Wait();
            sw.Stop();

            return new ActResult
                       {
                           SecondTargetRunnedAfter = secondTargetRunnedAfter,
                           FirstTargetThreadId = firstTargetThreadId,
                           SecondTargetThreadId = secondTargetThreadId
                       };
        }

        private class ActResult
        {
            public TimeSpan SecondTargetRunnedAfter { get; set; }
            public int FirstTargetThreadId { get; set; }
            public int SecondTargetThreadId { get; set; }
        }

        [Test]
        public void AwaitAllTargetsBroadcastMethodShouldSendToEachTargetAndAwaitForAllResults()
        {
            var result = ArrangeAndAct(BroadcastMethod.AwaitAllTargets, false);

            Assert.LessOrEqual(result.SecondTargetRunnedAfter.TotalMilliseconds, 100);
        }

        [Test]
        public void AwaitEachTargetBroadcastMethodShouldSendToEachTargetAndAwaitForResult()
        {
            var result = ArrangeAndAct(BroadcastMethod.AwaitEachTarget, false);

            Assert.GreaterOrEqual(result.SecondTargetRunnedAfter.TotalMilliseconds, 100);
        }

        [Test]
        public void CheckAllBroadcastMethodsForExist()
        {
            foreach (BroadcastMethod broadcastMethod in Enum.GetValues(typeof (BroadcastMethod)))
            {
                new BroadcastingTarget<string>(new[] {Mock.Create<ITarget<string>>()}, broadcastMethod);
            }
        }

        [Test]
        public void IfSetSendInThreadPoolFlagThenTargetMustBeRunnedInAnotherThread()
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var result = ArrangeAndAct(BroadcastMethod.AwaitAllTargets, true);

            Assert.AreNotEqual(currentThreadId, result.FirstTargetThreadId, "Id потоков совпадают!");
            Assert.AreNotEqual(currentThreadId, result.SecondTargetThreadId, "Id потоков совпадают!");
        }

        [Test]
        public void ShouldThrowExceptionIfBroadcastMethodIsUnknown()
        {
            Assert.Catch<ArgumentOutOfRangeException>(() => new BroadcastingTarget<string>(new[] {Mock.Create<ITarget<string>>()}, (BroadcastMethod) int.MaxValue));
        }
    }
}