using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Timeout(1000)]
    public class InfinityPipeTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            CreateInnerPipe();
        }

        #endregion

        private IItemsContainer<string> _itemsContainer;
        private Queue<string> _queue;

        public delegate bool OutAction<T>(out T arg);

        private void CreateInnerPipe()
        {
            _queue = new Queue<string>();

            _itemsContainer = Mock.Create<IItemsContainer<string>>();

            Mock.Arrange(() => _itemsContainer.TryAdd(null)).IgnoreArguments().Returns<string>(o =>
                                                                                            {
                                                                                                _queue.Enqueue(o);
                                                                                                return true;
                                                                                            });

            string item;
            Mock.Arrange(() => _itemsContainer.TryGet(out item)).DoInstead<OutAction<string>>((out string item2) =>
                                                                                                  {
                                                                                                      if (_queue.Count == 0)
                                                                                                      {
                                                                                                          item2 = null;
                                                                                                          return false;
                                                                                                      }
                                                                                                      item2 = _queue.Dequeue();
                                                                                                      return true;
                                                                                                  }).Returns<string>(o => o != null);
        }

        [Test]
        public void GetShouldBeBlockedWhenItemsContainerHasNoItems()
        {
            var blockingPipe = new InfinityPipe<string>(_itemsContainer);

            var t1 = blockingPipe.ReceiveAsync();

            Assert.AreEqual(TaskStatus.WaitingForActivation, t1.Status);
        }

        [Test]
        public void ShouldReturnDifferentItemsToEachAwaitedClients()
        {
            var blockingPipe = new InfinityPipe<string>(_itemsContainer);

            var t1 = blockingPipe.ReceiveAsync();
            var t2 = blockingPipe.ReceiveAsync();

            blockingPipe.SendAsync("1").Wait();
            blockingPipe.SendAsync("2").Wait();

            Assert.AreEqual("1", t1.Result);
            Assert.AreEqual("2", t2.Result);
        }

        [Test]
        public void GetShouldReturnItemImmediatlyWhenItemIsValid()
        {
            var blockingPipe = new InfinityPipe<string>(_itemsContainer);

            blockingPipe.SendAsync("1").Wait();

            var t1 = blockingPipe.ReceiveAsync();

            Assert.AreEqual("1", t1.Result);
        }

        [Test]
        public void GetShouldReturnItemToAwaiterEvenIfFirstTryWasException()
        {
            int callCount = 0;

            string item;
            Mock.Arrange(() => _itemsContainer.TryGet(out item))
                .DoInstead<OutAction<string>>((out string item2) =>
                                                  {
                                                      callCount++;
                                                      if (callCount == 3)
                                                      {
                                                          throw new Exception();
                                                      }

                                                      if (_queue.Count == 0)
                                                      {
                                                          item2 = null;
                                                          return false;
                                                      }
                                                      item2 = _queue.Dequeue();
                                                      return true;
                                                  }).Returns<string>(o => o != null);

            var blockingPipe = new InfinityPipe<string>(_itemsContainer);
            var t1 = blockingPipe.ReceiveAsync();

            Assert.Catch(() => blockingPipe.SendAsync("1").Wait());
            Assert.AreEqual(TaskStatus.WaitingForActivation, t1.Status);

            blockingPipe.SendAsync("1").Wait();

            Assert.AreEqual("1", t1.Result);
        }

        [Test]
        public void WhenSendBatchPipeShouldReturnItemToAwaiterOnlyWhenAllItemsInBatchWillBeAdded()
        {
            string receivedItem = null;
            int itemsContainerCount = -1;

            var blockingPipe = new InfinityPipe<string>(_itemsContainer);
            var t1 = blockingPipe.ReceiveAsync();
            t1.ContinueWith(o =>
                                {
                                    receivedItem = o.Result;
                                    itemsContainerCount = _queue.Count;
                                }, TaskContinuationOptions.ExecuteSynchronously);

            blockingPipe.SendAsync(new[] {"1", "2"}).Wait();

            Assert.AreEqual("1", receivedItem);
            Assert.AreEqual(1, itemsContainerCount);
        }
    }
}