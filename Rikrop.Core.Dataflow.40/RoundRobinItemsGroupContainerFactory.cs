namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика для создания объектов <see cref="RoundRobinItemsGroupContainer{TRoundRobinKey,TItem}"/>.
    /// </summary>
    /// <typeparam name="TRoundRobinKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class RoundRobinItemsGroupContainerFactory<TRoundRobinKey, TItem> : IItemsContainerFactory<TRoundRobinKey, TItem>
    {
        private readonly IItemsContainerFactory<TRoundRobinKey, TItem> _itemsContainerFactory;
        private readonly IConverter<TItem, TRoundRobinKey> _itemToKeyConverter;

        /// <summary>
        /// Конструктор для создания <see cref="RoundRobinItemsGroupContainerFactory{TRoundRobinKey,TItem}"/>.
        /// </summary>
        /// <param name="itemsContainerFactory">Фабрика создания контейнеров для групп.</param>
        /// <param name="itemToKeyConverter">Извлекатель ключей из элементов, по которым происходит группировка элементов.</param>
        public RoundRobinItemsGroupContainerFactory(IItemsContainerFactory<TRoundRobinKey, TItem> itemsContainerFactory, IConverter<TItem, TRoundRobinKey> itemToKeyConverter)
        {
            _itemsContainerFactory = itemsContainerFactory;
            _itemToKeyConverter = itemToKeyConverter;
        }

        public IItemsContainer<TItem> Create(TRoundRobinKey key)
        {
            return new RoundRobinItemsGroupContainer<TRoundRobinKey, TItem>(_itemsContainerFactory, _itemToKeyConverter);
        }
    }
}