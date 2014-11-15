using System.Collections.Concurrent;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="ItemsQueueContainer{TItem}"/> - это потокобезопасный контейнер элементов, основанный на очереди.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ItemsQueueContainer<TItem> : IItemsContainer<TItem>
    {
        private readonly ConcurrentQueue<TItem> _items;

        public int Count
        {
            get { return _items.Count; }
        }

        public ItemsQueueContainer()
        {
            _items = new ConcurrentQueue<TItem>();
        }

        public bool TryGet(out TItem item)
        {
            return _items.TryDequeue(out item);
        }

        public bool TryAdd(TItem item)
        {
            _items.Enqueue(item);
            return true;
        }
    }
}