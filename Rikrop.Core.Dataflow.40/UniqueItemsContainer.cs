using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Реализует поддержку уникальности элементов во внутреннем <see cref="IItemsContainer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class UniqueItemsContainer<TItem> : IItemsContainer<TItem>
    {
        private readonly IItemsContainer<TItem> _itemsContainer;
        private readonly ConcurrentDictionary<TItem, TItem> _itemsInQueue;

        public int Count
        {
            get { return _itemsContainer.Count; }
        }

        /// <summary>
        /// Создаёт объект <see cref="UniqueItemsContainer{TItem}"/>
        /// </summary>
        /// <param name="itemsContainer">Контейнер, над элементами которого будет проверяться уникальность.</param>
        /// <param name="equalityComparer">Сравниватель элементов друг с другом.</param>
        public UniqueItemsContainer(IItemsContainer<TItem> itemsContainer, IEqualityComparer<TItem> equalityComparer)
        {
            Contract.Requires<ArgumentNullException>(itemsContainer != null);
            Contract.Requires<ArgumentNullException>(equalityComparer != null);

            _itemsContainer = itemsContainer;
            _itemsInQueue = new ConcurrentDictionary<TItem, TItem>(Environment.ProcessorCount * 3, 4, equalityComparer);
        }

        public bool TryGet(out TItem item)
        {
            if (!_itemsContainer.TryGet(out item))
            {
                return false;
            }

            TItem dictItem;
            return _itemsInQueue.TryRemove(item, out dictItem);
        }

        /// <summary>
        /// Добавляет элемент во внутренний <see cref="IItemsContainer{TItem}"/>, если такого элемента там ещё нет. Возвращает true, если элемент уникален и добавлен во внутренний контейнер. Иначе false.
        /// </summary>
        /// <param name="item">Элемент для проверки на уникальность и добавления во внутренний контецнер.</param>
        /// <returns>True, если элемент уникален и добавлен во внутренний контейнер. Иначе false.</returns>
        public bool TryAdd(TItem item)
        {
            if (_itemsInQueue.TryAdd(item, item))
            {
                if (!_itemsContainer.TryAdd(item))
                {
                    _itemsInQueue.TryRemove(item, out item);
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}