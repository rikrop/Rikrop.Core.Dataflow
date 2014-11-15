namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика создания <see cref="ItemsStackContainer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class ItemsStackContainerFactory<TKey, TItem> : IItemsContainerFactory<TKey, TItem>
    {
        public IItemsContainer<TItem> Create(TKey key)
        {
            return new ItemsStackContainer<TItem>();
        }
    }
}