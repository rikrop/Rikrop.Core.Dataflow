using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Rikrop.Core.Dataflow
{
    /// <summary>
    /// <see cref="ItemsByDateCollection{TItem}"/> - это коллекция, хранящая элементы с метками времени. 
    /// Используется в <see cref="ScheduledTransmitterItem{TItem}"/>. 
    /// </summary>
    /// <remarks>НЕ ПОТОКОБЕЗОПАСЕН!</remarks>
    /// <typeparam name="TItem"></typeparam>
    public class ItemsByDateCollection<TItem>
    {
        private readonly SortedDictionary<DateTime, List<TItem>> _dateGroups;
        private readonly Dictionary<TItem, DateTime> _items;

        /// <summary>
        /// Возвращает кол-во элементов в коллекции.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        public ItemsByDateCollection()
        {
            _dateGroups = new SortedDictionary<DateTime, List<TItem>>();
            _items = new Dictionary<TItem, DateTime>();
        }

        /// <summary>
        /// Удаляет элемент из коллекции
        /// </summary>
        /// <param name="item">Элемент для удаления.</param>
        public void Remove(TItem item)
        {
            Contract.Requires<ArgumentNullException>(!Equals(item, null));

            DateTime itemDate;
            if (_items.TryGetValue(item, out itemDate))
            {
                _items.Remove(item);
                RemoveItemFromDateGroup(itemDate, item);
            }
        }

        /// <summary>
        /// Добавляет элемент в коллекцию.
        /// </summary>
        /// <param name="item">Элемент для добавления.</param>
        /// <param name="itemDate">Метка времени, соответствующая <paramref name="item"/></param>
        public void Add(TItem item, DateTime itemDate)
        {
            Contract.Requires<ArgumentNullException>(!Equals(item, null));

            DateTime oldDate;
            if (!_items.TryGetValue(item, out oldDate))
            {
                _items.Add(item, itemDate);
                SaveItemToDateGroup(itemDate, item);
            }
            else
            {
                if (itemDate != oldDate)
                {
                    RemoveItemFromDateGroup(oldDate, item);
                    SaveItemToDateGroup(itemDate, item);
                }
            }
        }

        /// <summary>
        /// Удаляет из коллекции элементы, метки времени которых меньше либо равны <paramref name="tillDate"/>. Возвращает список удалённых эелементов.
        /// </summary>
        /// <param name="tillDate">Дата и время до которого необходимо удалить элементы</param>
        /// <returns>Возвращает список удалённых эелементов.</returns>
        public IEnumerable<TItem> TakeItemsTill(DateTime tillDate)
        {
            var expiredItems = new List<TItem>();

            foreach (var expiredDate in GetExpiredDates(tillDate))
            {
                var expiredGroup = _dateGroups[expiredDate];
                expiredItems.AddRange(expiredGroup);
                _dateGroups.Remove(expiredDate);

                foreach (var item in expiredGroup)
                {
                    _items.Remove(item);
                }
            }

            return expiredItems;
        }

        /// <summary>
        /// Возвращает самую маленькую метку времени, хранящуюся в коллекции. Если в коллекции нет элементов, возвращает false.
        /// </summary>
        /// <param name="date">Самая маленькая меткуа времени, хранящуяся в коллекции.</param>
        /// <returns>True, если в коллекции есть элементы. Иначе false.</returns>
        public bool TryGetNearestDate(out DateTime date)
        {
            if (_dateGroups.Keys.Count > 0)
            {
                date = _dateGroups.Keys.First();
                return true;
            }
            date = default(DateTime);
            return false;
        }

        private void SaveItemToDateGroup(DateTime scheduleDate, TItem item)
        {
            List<TItem> itemsGroup;
            if (!_dateGroups.TryGetValue(scheduleDate, out itemsGroup))
            {
                itemsGroup = new List<TItem>();
                _dateGroups.Add(scheduleDate, itemsGroup);
            }
            
            itemsGroup.Add(item);
        }

        private void RemoveItemFromDateGroup(DateTime itemDate, TItem item)
        {
            List<TItem> itemsGroup;
            if (_dateGroups.TryGetValue(itemDate, out itemsGroup))
            {
                itemsGroup.Remove(item);
                if (!itemsGroup.Any())
                {
                    _dateGroups.Remove(itemDate);
                }
            }
        }

        private IEnumerable<DateTime> GetExpiredDates(DateTime now)
        {
            var keys = new List<DateTime>();
            foreach (var key in _dateGroups.Keys)
            {
                if (key <= now)
                {
                    keys.Add(key);
                }
                else
                {
                    break;
                }
            }
            return keys;
        }
    }
}