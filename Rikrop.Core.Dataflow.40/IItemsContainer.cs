using System;
using System.Diagnostics.Contracts;
using Rikrop.Core.Dataflow.Contracts;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Интерфейс, описывающий абстрактный контейнер элементов.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    [ContractClass(typeof (ContractIItemsContainer<>))]
    public interface IItemsContainer<TItem>
    {
        /// <summary>
        /// Возвращает кол-во элементов в контейнере.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Добавляет элемент в контейнер. Возвращает true, если элемент добавлен.
        /// </summary>
        /// <param name="item">Элемент для добавления.</param>
        /// <returns>True, если элемент добавлен, иначе false.</returns>
        bool TryAdd(TItem item);

        /// <summary>
        /// Удаляет элемент из контейнера. Возвращает true, если элемент удалён.
        /// </summary>
        /// <param name="item">Элемент для удаления.</param>
        /// <returns>True, если элемент удалён, иначе false.</returns>
        bool TryGet(out TItem item);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof (IItemsContainer<>))]
        public abstract class ContractIItemsContainer<TItem> : IItemsContainer<TItem>
        {
            int IItemsContainer<TItem>.Count
            {
                get
                {
                    Contract.Assume(Contract.Result<int>() >= 0);
                    return default(int);
                }
            }

            public bool TryAdd(TItem item)
            {
                Contract.Requires<ArgumentNullException>(!Equals(item, null));
                return default(bool);
            }

            public bool TryGet(out TItem item)
            {
                item = default(TItem);
                Contract.Assume(!Contract.Result<bool>() || !Equals(item, null));
                return default(bool);
            }
        }
    }
}