using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Фабрика для создания <see cref="UniqueItemsContainer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class UniqueItemsContainerFactory<TKey, TItem> : IItemsContainerFactory<TKey, TItem>
    {
        private readonly IItemsContainerFactory<TKey, TItem> _itemsContainerFactory;
        private readonly IEqualityComparer<TItem> _equalityComparer;

        /// <summary>
        /// Создаёт объект <see cref="UniqueItemsContainerFactory{TKey,TItem}"/>
        /// </summary>
        /// <param name="itemsContainerFactory">Фабрика контейнеров, над элементами которых будет проверяться уникальность.</param>
        /// <param name="equalityComparer">Сравниватель элементов друг с другом.</param>
        public UniqueItemsContainerFactory(IItemsContainerFactory<TKey, TItem> itemsContainerFactory, IEqualityComparer<TItem> equalityComparer)
        {
            Contract.Requires<ArgumentNullException>(itemsContainerFactory != null);

            _itemsContainerFactory = itemsContainerFactory;
            _equalityComparer = equalityComparer;
        }

        public IItemsContainer<TItem> Create(TKey key)
        {
            return new UniqueItemsContainer<TItem>(_itemsContainerFactory.Create(key), _equalityComparer);
        }
    }
}