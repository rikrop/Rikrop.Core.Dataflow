using System.Collections.Concurrent;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="ItemsQueueContainer{TItem}"/> - это потокобезопасный контейнер элементов, основанный на стеке.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ItemsStackContainer<TItem> : IItemsContainer<TItem>
    {
        private readonly ConcurrentStack<TItem> _items;

        public int Count
        {
            get { return _items.Count; }
        }

        public ItemsStackContainer()
        {
            _items = new ConcurrentStack<TItem>();
        }

        public bool TryGet(out TItem item)
        {
            return _items.TryPop(out item);
        }

        public bool TryAdd(TItem item)
        {
            _items.Push(item);
            return true;
        }
    }
}