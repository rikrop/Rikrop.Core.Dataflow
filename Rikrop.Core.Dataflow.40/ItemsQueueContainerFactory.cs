namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика создания <see cref="ItemsQueueContainer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class ItemsQueueContainerFactory<TKey, TItem> : IItemsContainerFactory<TKey, TItem>
    {
        public IItemsContainer<TItem> Create(TKey key)
        {
            return new ItemsQueueContainer<TItem>();
        }
    }
}