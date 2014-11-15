using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="RoundRobinItemsGroupContainer{TRoundRobinKey,TItem}"/> - контейнер элементов, который группирует элементы внутри себя по ключу и отдаёт элементы по алгоритму RoundRobin между этими группами.
    /// </summary>
    /// <typeparam name="TRoundRobinKey">Тип ключа группировки</typeparam>
    /// <typeparam name="TItem">Тип элементов</typeparam>
    public class RoundRobinItemsGroupContainer<TRoundRobinKey, TItem> : IItemsContainer<TItem>
    {
        private readonly IItemsContainerFactory<TRoundRobinKey, TItem> _itemsContainerFactory;
        private readonly IConverter<TItem, TRoundRobinKey> _itemToKeyConverter;
        private readonly ConcurrentDictionary<TRoundRobinKey, IItemsContainer<TItem>> _grouppedQueues;
        private readonly ConcurrentQueue<IItemsContainer<TItem>> _rrQueue;

        /// <summary>
        /// Конструктор для создания <see cref="RoundRobinItemsGroupContainer{TRoundRobinKey,TItem}"/>.
        /// </summary>
        /// <param name="itemsContainerFactory">Фабрика создания контейнеров для групп.</param>
        /// <param name="itemToKeyConverter">Извлекатель ключей из элементов, по которым происходит группировка элементов.</param>
        public RoundRobinItemsGroupContainer(IItemsContainerFactory<TRoundRobinKey, TItem> itemsContainerFactory, IConverter<TItem, TRoundRobinKey> itemToKeyConverter)
        {
            Contract.Requires<ArgumentNullException>(itemsContainerFactory != null);
            Contract.Requires<ArgumentNullException>(itemToKeyConverter != null);

            _itemsContainerFactory = itemsContainerFactory;
            _itemToKeyConverter = itemToKeyConverter;

            _grouppedQueues = new ConcurrentDictionary<TRoundRobinKey, IItemsContainer<TItem>>();
            _rrQueue = new ConcurrentQueue<IItemsContainer<TItem>>();
        }

        private int _count;
        public int Count
        {
            get { return _count; }
        }

        public bool TryAdd(TItem item)
        {
            var enqueued = false;
            _grouppedQueues.AddOrUpdate(key: _itemToKeyConverter.Convert(item),
                                        addValueFactory: key =>
                                                             {
                                                                 var rrItem = _itemsContainerFactory.Create(key);
                                                                 enqueued = rrItem.TryAdd(item);
                                                                 _rrQueue.Enqueue(rrItem);
                                                                 return rrItem;
                                                             },
                                        updateValueFactory: (key, byKeyGroup) =>
                                                                {
                                                                    enqueued = byKeyGroup.TryAdd(item);
                                                                    return byKeyGroup;
                                                                });
            if (enqueued)
            {
                Interlocked.Increment(ref _count);
            }
            return enqueued;
        }

        public bool TryGet(out TItem item)
        {
            var groupsCount = _rrQueue.Count;

            for (var i = 0; i < groupsCount; i++)
            {
                IItemsContainer<TItem> itemsContainer;
                if (_rrQueue.TryDequeue(out itemsContainer))
                {
                    try
                    {
                        if (itemsContainer.TryGet(out item))
                        {
                            Interlocked.Decrement(ref _count);
                            return true;
                        }
                    }
                    finally
                    {
                        _rrQueue.Enqueue(itemsContainer);
                    }
                }
            }

            item = default(TItem);
            return false;
        }
    }
}