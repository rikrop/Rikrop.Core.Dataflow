using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Rikrop.Core.Framework;
using Rikrop.Core.Framework.Exceptions;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Timeout(2000)]
    public class ScheduledTransmitterTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _source = Mock.Create<ISource<ScheduledTransmitterItem<int>>>();
            _target = Mock.Create<ITarget<int>>();
            _exceptionHandler = Mock.Create<IExceptionHandler>();
            _dateTimeProvider = Mock.Create<IDateTimeProvider>();
            _timer = Mock.Create<ITimer>();
            _sendedItemsCount = 0;

            _transmitter = new ScheduledTransmitter<int>(_source, _target, _dateTimeProvider, _timer, _exceptionHandler);
        }

        #endregion

        private ISource<ScheduledTransmitterItem<int>> _source;
        private ITarget<int> _target;
        private IExceptionHandler _exceptionHandler;
        private IDateTimeProvider _dateTimeProvider;
        private ScheduledTransmitter<int> _transmitter;

        private int _sendedItemsCount;
        private ITimer _timer;

        private void Act(DateTime nowDate, DateTime nextDate)
        {
            Mock.Arrange(() => _dateTimeProvider.Now).Returns(nowDate);

            _transmitter.StartAsync();

            Mock.Arrange(() => _dateTimeProvider.Now).Returns(nextDate);
            Mock.Raise(() => _timer.Tick += null, EventArgs.Empty);
        }

        private void Arrange(int item, int sendItemsCount, DateTime nowDate)
        {
            var returnCount = 0;
            var timeout = 200;

            Mock.Arrange(() => _source.ReceiveAsync()).Returns(() =>
                                                                   {
                                                                       if (returnCount < sendItemsCount)
                                                                       {
                                                                           timeout += returnCount*100;
                                                                           returnCount++;
                                                                           return Task.FromResult(new ScheduledTransmitterItem<int>(nowDate.AddMilliseconds(timeout), item + returnCount));
                                                                       }
                                                                       return new TaskCompletionSource<ScheduledTransmitterItem<int>>().Task;
                                                                   });

            Mock.Arrange(() => _target.SendAsync(0)).IgnoreArguments().Returns<int>(o =>
                                                                                        {
                                                                                            _sendedItemsCount++;
                                                                                            return Task.FromResult(true);
                                                                                        });
        }

        [Test]
        public void ShouldSendItemsInBatchIfTheyTimeIsCloseToSecond()
        {
            const int item = 255;
            const int sendItemsCount = 4;
            var nowDate = new DateTime(2013, 05, 08, 12, 00, 00, 800);
            var nextDate = new DateTime(2013, 05, 08, 13, 00, 00);

            Arrange(item, sendItemsCount, nowDate);
            Act(nowDate, nextDate);

            Assert.AreEqual(sendItemsCount, _sendedItemsCount);
        }

        [Test]
        public void ShouldSendItemToTargetWhenTimeHasCome()
        {
            const int item = 255;
            const int sendItemsCount = 1;
            var nowDate = new DateTime(2013, 05, 08, 12, 00, 00, 800);
            var nextDate = new DateTime(2013, 05, 08, 11, 00, 00, 000);

            Arrange(item, sendItemsCount, nowDate);
            Act(nowDate, nextDate);

            Assert.AreEqual(0, _sendedItemsCount);
            
            nextDate = new DateTime(2013, 05, 08, 13, 00, 00);
            Act(nowDate, nextDate);

            Assert.AreEqual(1, _sendedItemsCount);
        }

        [Test]
        public void ShouldNotSendItemIfSourceReturnStopSchedule()
        {
            const int item = 255;
            const int sendItemsCount = 2;
            const int timeout = 200;
            int returnCount = 0;
            var nowDate = new DateTime(2013, 05, 08, 12, 00, 00, 800);
            var nextDate = new DateTime(2013, 05, 08, 13, 00, 00);

            Arrange(item, sendItemsCount, nowDate);
            Mock.Arrange(() => _source.ReceiveAsync()).Returns(() =>
                                                                   {
                                                                       if (returnCount == 0)
                                                                       {
                                                                           returnCount++;
                                                                           return Task.FromResult(new ScheduledTransmitterItem<int>(nowDate.AddMilliseconds(timeout), item));
                                                                       }
                                                                       if(returnCount == 1)
                                                                       {
                                                                           returnCount++;
                                                                           return Task.FromResult(new ScheduledTransmitterItem<int>(nowDate.AddMilliseconds(timeout), item, true));
                                                                       }
                                                                       return new TaskCompletionSource<ScheduledTransmitterItem<int>>().Task;
                                                                   });

            Act(nowDate, nextDate);

            Task.Delay(300).Wait();

            Assert.AreEqual(2, returnCount);
            Assert.AreEqual(0, _sendedItemsCount);
        }
    }
}