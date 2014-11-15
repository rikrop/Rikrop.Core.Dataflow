using System.Collections.Generic;
using System.Linq;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// Набор вспомогательных методов для создания <see cref="SourceResponse{TItem}"/>.
    /// </summary>
    public static class SourceResponses
    {
        /// <summary>
        /// Создаёт <see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = true.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <returns><see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = true.</returns>
        public static SourceResponse<TItem> EmptyResponse<TItem>()
        {
            return new SourceResponse<TItem>();
        }

        /// <summary>
        /// Создаёт <see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = false и инициализированным элементом.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item">Элемент для инициализации <see cref="SourceResponse{TItem}"/></param>
        /// <returns><see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = false и инициализированным элементом.</returns>
        public static SourceResponse<TItem> NotEmptyResponse<TItem>(TItem item)
        {
            return new SourceResponse<TItem>(item);
        }

        /// <summary>
        /// Создаёт <see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = true, если <paramref name="item"/> = null.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item">Элемент для выбора, какой <see cref="SourceResponse{TItem}"/> создавать.</param>
        /// <returns><see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = true, если <paramref name="item"/> = null.</returns>
        public static SourceResponse<TItem> NullBasedResponse<TItem>(TItem item)
            where TItem : class
        {
            return item == null
                       ? EmptyResponse<TItem>()
                       : NotEmptyResponse(item);
        }

        /// <summary>
        /// Создаёт <see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = true, если <paramref name="items"/> = null или кол-во элементов в <paramref name="items"/> = 0.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items">Набор элементов для выбора, какой <see cref="SourceResponse{TItem}"/> создавать.</param>
        /// <returns><see cref="SourceResponse{TItem}"/> с флагом <see cref="SourceResponse{TItem}.IsSourceEmpty"/> = true, если <paramref name="items"/> = null или кол-во элементов в <paramref name="items"/> = 0.</returns>
        public static SourceResponse<IEnumerable<TItem>> CollectionBasedResponse<TItem>(IEnumerable<TItem> items)
        {
            return items == null || !items.Any()
                       ? EmptyResponse<IEnumerable<TItem>>()
                       : NotEmptyResponse(items);
        }
    }
}