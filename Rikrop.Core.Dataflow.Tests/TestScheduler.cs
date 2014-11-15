using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rikrop.Core.Dataflow.Build;
using Rikrop.Core.Framework;
using Rikrop.Core.Framework.Exceptions;
using NUnit.Framework;
using Telerik.JustMock;

namespace Rikrop.Core.Dataflow.Tests
{
    [TestFixture, Ignore]
    public class TestScheduler
    {
        [Test]
        public void TestMethod2()
        {
            var exceptionHandler = Mock.Create<IExceptionHandler>();
            Mock.Arrange(() => exceptionHandler.Handle(null)).IgnoreArguments().Returns<Exception>(o =>
                                                                                                       {
                                                                                                           Console.WriteLine(o);
                                                                                                           return true;
                                                                                                       });

            var itemStore = new ItemStore();
            var uniqueQueue = new ItemsQueueContainer<ItemWithDate>();
            var infinityPipe = new InfinityPipe<ItemWithDate>(uniqueQueue);

            var dataflow = DataflowBuilder
                .StartFrom(infinityPipe)
                .ConvertItemsTo(o => new ScheduledTransmitterItem<int>(o.DateTime, o.Num, stopSchedule: o.Count == 4))
                .TransmitWith(o => o.ScheduledTransmitter(new DateTimeProvider(), exceptionHandler))
                .Transform(itemStore.GetItem)
                .BroadcastTo(t1 => t1.SendToTarget(new LoggingTarget<ItemWithDate>()),
                             t2 => t2.Transform(o =>
                                                    {
                                                        o.Count++;
                                                        o.DateTime = DateTime.Now.AddSeconds(o.Num + 1);
                                                        return o;
                                                    })
                                     .RouteTo(condition => condition.Case(o => o.Count == 4)
                                                                    .BroadcastTo(t21 => t21.SendToTarget(new DeleteTarget(itemStore)),
                                                                                 t22 => t22.SendToTarget(infinityPipe)),

                                              condition => condition.Default()
                                                                    .BroadcastTo(t21 => t21.SendToTarget(new UpdateTarget(itemStore)),
                                                                                 t22 => t22.SendToTarget(infinityPipe))))
                .Build();

            dataflow.Start();

            var items = itemStore.Load();
            foreach (var item in items)
            {
                infinityPipe.SendAsync(item).Wait();
            }

            Task.Delay(10000).Wait();
        }

    }

    public class UpdateTarget : ITarget<ItemWithDate>
    {
        private readonly ItemStore _itemStore;

        public UpdateTarget(ItemStore itemStore)
        {
            _itemStore = itemStore;
        }

        public Task SendAsync(ItemWithDate item)
        {
            return _itemStore.Update(item);
        }
    }

    public class DeleteTarget : ITarget<ItemWithDate>
    {
        private readonly ItemStore _itemStore;

        public DeleteTarget(ItemStore itemStore)
        {
            _itemStore = itemStore;
        }

        public Task SendAsync(ItemWithDate item)
        {
            return _itemStore.Delete(item);
        }
    }

    public class ItemStore
    {
        private readonly ConcurrentDictionary<int, ItemWithDate> _items = new ConcurrentDictionary<int, ItemWithDate>();

        public IReadOnlyCollection<ItemWithDate> Load()
        {
            foreach (var num in Enumerable.Range(0, 10))
            {
                _items.TryAdd(num, new ItemWithDate(num));
            }

            return _items.Values.ToArray();
        }

        public ItemWithDate GetItem(int num)
        {
            ItemWithDate item;
            if (_items.TryGetValue(num, out item))
            {
                return item;
            }

            throw new InvalidOperationException("!!!");
        }

        public Task Update(ItemWithDate item)
        {
            _items[item.Num] = item;
            return Task.FromResult(true);
        }

        public Task Delete(ItemWithDate item)
        {
            ItemWithDate temp;
            _items.TryRemove(item.Num, out temp);
            return Task.FromResult(true);
        }
    }

    public class DatabaseTarget : ITarget<ItemWithDate>
    {
        public Task SendAsync(ItemWithDate item)
        {
            return Task.FromResult(true);
        }
    }

    //public class ScheduledPipe<TItem> : IPipe<TItem>
    //{
    //    private readonly ITarget<TItem> _target;
    //    private readonly IPipe<TItem> _pipe;

    //    public ScheduledPipe(ITarget<TItem> target, IPipe<TItem> pipe)
    //    {
    //        _target = target;
    //        _pipe = pipe;
    //    }

    //    public Task<TItem> ReceiveAsync()
    //    {
    //        return _pipe.ReceiveAsync();
    //    }

    //    public Task SendAsync(TItem item)
    //    {
    //        _target.SendAsync(item);
    //        _pipe.SendAsync(item);
    //    }

    //    async Task<ScheduledTransmitterItem<TItem>> ISource<ScheduledTransmitterItem<TItem>>.ReceiveAsync()
    //    {
    //        var item = await ReceiveAsync();

    //        return new ScheduledTransmitterItem<TItem>(_dateExtractor.GetKey(item), item);
    //    }
    //}

    public class SchedulerSource : ISource<ItemWithDate>
    {
        private int _count;
        public Task<ItemWithDate> ReceiveAsync()
        {
            if (_count == 10)
            {
                return Task.FromResult<ItemWithDate>(null);
            }
            _count++;

            return Task.FromResult(new ItemWithDate(_count, DateTime.Now));
        }
    }

    public class ItemWithDate
    {
        private readonly int _num;
        private DateTime _dateTime;


        public ItemWithDate(int num)
        {
            _num = num;
            _dateTime = DateTime.Now.AddSeconds(num + 1);
        }

        public ItemWithDate(int num, DateTime dateTime)
        {
            _num = num;
            _dateTime = dateTime;
        }

        public DateTime DateTime
        {
            get { return _dateTime; }
        set { _dateTime = value; }
        }

        public int Num
        {
            get { return _num; }
        }

        public int Count { get; set; }

        public override string ToString()
        {
            return Num.ToString();
        }
    }
}