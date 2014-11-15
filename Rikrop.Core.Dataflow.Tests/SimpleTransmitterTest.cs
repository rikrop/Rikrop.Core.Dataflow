using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rikrop.Core.Framework.Exceptions;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Timeout(5000)]
    public class SimpleTransmitterTest
    {
        private Queue<string> _sourceQueue;
        private ISource<string> _source;
        private ITarget<string> _target;
        private IExceptionHandler _exceptionHandler;
        private List<string> _targetQueue;

        [SetUp]
        public void Setup()
        {
            _sourceQueue = new Queue<string>();
            _targetQueue = new List<string>();

            _source = Mock.Create<ISource<string>>();
            _target = Mock.Create<ITarget<string>>();
            _exceptionHandler = Mock.Create<IExceptionHandler>();


            Mock.Arrange(() => _target.SendAsync(null)).IgnoreArguments().Returns<string>(o =>
                                                                                              {
                                                                                                  _targetQueue.Add(o);
                                                                                                  return Task.FromResult(true);
                                                                                              });

            Mock.Arrange(() => _source.ReceiveAsync()).Returns(() => _sourceQueue.Count == 0
                                                                         ? new TaskCompletionSource<string>().Task
                                                                         : Task.FromResult(_sourceQueue.Dequeue()));
            Mock.Arrange(() => _exceptionHandler.Handle(null)).IgnoreArguments().Returns(true);
        }

        [Test]
        public void ShouldTransferItemsWhileSourceNotReturnEmptyResponse()
        {
            var itemsTransfer = new SimpleTransmitter<string>(_source, _target, _exceptionHandler);

            _sourceQueue.Enqueue("1");
            _sourceQueue.Enqueue("2");

            itemsTransfer.StartAsync();

            SpinWait.SpinUntil(() => _sourceQueue.Count == 0);

            Assert.AreEqual(new[] {"1", "2"}, _targetQueue);
        }

        [Test]
        public void ShouldContinueTransferItemsIfExceptionWasHandled()
        {
            Mock.Arrange(() => _target.SendAsync(null)).IgnoreArguments().Returns(() => { throw new Exception(); });
            Mock.Arrange(() => _exceptionHandler.Handle(null)).IgnoreArguments().Returns(true);

            var itemsTransfer = new SimpleTransmitter<string>(_source, _target, _exceptionHandler);

            _sourceQueue.Enqueue("1");
            _sourceQueue.Enqueue("2");

            itemsTransfer.StartAsync();

            SpinWait.SpinUntil(() => _sourceQueue.Count == 0);

            Assert.AreEqual(new string[0], _sourceQueue);
        }

        [Test]
        public void ShouldStopTransferItemsIfExceptionWasNotHandled()
        {
            Mock.Arrange(() => _target.SendAsync(null)).IgnoreArguments().Returns(() => { throw new Exception(); });
            Mock.Arrange(() => _exceptionHandler.Handle(null)).IgnoreArguments().Returns(false);

            var itemsTransfer = new SimpleTransmitter<string>(_source, _target, _exceptionHandler);

            _sourceQueue.Enqueue("1");
            _sourceQueue.Enqueue("2");

            itemsTransfer.StartAsync();

            SpinWait.SpinUntil(() => _sourceQueue.Count == 1);

            Assert.AreEqual(new[] {"2"}, _sourceQueue);
        }

        [Test]
        public void ShouldStopTransferItemsIfStopTransferIsForced()
        {
            var taskCompletetionSource = new TaskCompletionSource<bool>();

            Mock.Arrange(() => _target.SendAsync(null)).IgnoreArguments().Returns(() =>
                                                                                      {
                                                                                          taskCompletetionSource.Task.Wait();
                                                                                          return Task.FromResult(true);
                                                                                      });
            Mock.Arrange(() => _exceptionHandler.Handle(null)).IgnoreArguments().Returns(false);

            var itemsTransfer = new SimpleTransmitter<string>(_source, _target, _exceptionHandler);

            _sourceQueue.Enqueue("1");
            _sourceQueue.Enqueue("2");

            var transferTask = Task.Run(()=> itemsTransfer.StartAsync());

            SpinWait.SpinUntil(() => _sourceQueue.Count == 1);

            itemsTransfer.Stop();
            taskCompletetionSource.SetResult(true);
            transferTask.Wait();

            Assert.AreEqual(new[] { "2" }, _sourceQueue);
        }
    }
}