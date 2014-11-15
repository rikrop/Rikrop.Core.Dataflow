using System;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Представляет собой ответ источника, который может содержать элемент источника, либо информацию о том, что источник пуст.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public struct SourceResponse<TItem>
    {
        private readonly TItem _item;
        private readonly bool _hasItem;

        /// <summary>
        /// Возвращает флаг, обосзначающий наличие элементов в источнике.
        /// </summary>
        public bool IsSourceEmpty
        {
            get { return !_hasItem; }
        }

        /// <summary>
        /// Возвращает элемент, если источник ещё не пуст
        /// </summary>
        /// <exception cref="InvalidOperationException">Нельзя обратиться к свойству <see cref="Item"/>, когда <see cref="IsSourceEmpty"/> = true</exception>
        public TItem Item
        {
            get
            {
                if (IsSourceEmpty)
                {
                    throw new InvalidOperationException("Нельзя обратиться к свойству Item, когда IsSourceEmpty = true");
                }
                return _item;
            }
        }

        /// <summary>
        /// Создлаёт объект <see cref="SourceResponse{TItem}"/> инициализированный элементом.
        /// </summary>
        /// <param name="item">Элемент из источника.</param>
        public SourceResponse(TItem item)
        {
            _item = item;
            _hasItem = true;
        }
    }
}